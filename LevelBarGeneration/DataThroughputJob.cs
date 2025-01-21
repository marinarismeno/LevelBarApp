// <copyright file="DataThroughputJob.cs" company="VIBES.technology">
// Copyright (c) VIBES.technology. All rights reserved.
// </copyright>

namespace LevelBarGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MathNet.Numerics;
    using MathNet.Numerics.Distributions;

    /// <summary>
    /// MockClientDataThrptJob class
    /// </summary>
    public class DataThroughputJob
    {
        // Fields
        private static int samplingRate;
        private static int channelBlockSize;
        private static double samplingTime;
        private static int numberOfChannels;

        private static float[][] levels = null;
        private static int[] channelIds = null;
        private static int[] numsRawData;
        private static int jobCounter = 0;

        private CancellationTokenSource _cancellationTokenSource;
        private Task _schedulerTask;

        // Methods

        /// <summary>
        /// Setups the job.
        /// </summary>
        /// <param name="samplingRate">The sampling rate.</param>
        /// <param name="channelBlockSize">Size of the channel block.</param>
        /// <param name="samplingTime">The sampling time.</param>
        /// <param name="numberOfChannels">The number of channels.</param>
        public static void SetupJob(int samplingRate, int channelBlockSize, double samplingTime, int numberOfChannels)
        {
            // Set the data
            DataThroughputJob.samplingRate = samplingRate;
            DataThroughputJob.channelBlockSize = channelBlockSize;
            DataThroughputJob.samplingTime = samplingTime;
            DataThroughputJob.numberOfChannels = numberOfChannels;
            GenerateData(out levels, out channelIds, out numsRawData);
        }

        /// <summary>
        /// Starts the job scheduler with a specified interval.
        /// </summary>
        /// <param name="intervalMilliseconds">The interval in milliseconds.</param>
        public void StartScheduler(int intervalMilliseconds)
        {
            if (_cancellationTokenSource != null)
            {
                throw new InvalidOperationException("Scheduler is already running.");
            }

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            _schedulerTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        ExecuteJob();
                        await Task.Delay(intervalMilliseconds, token);
                    }
                    catch (TaskCanceledException)
                    {
                        // Task was canceled; exit gracefully.
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error during job execution: {ex.Message}");
                    }
                }
            }, token);
        }

        /// <summary>
        /// Stops the job scheduler.
        /// </summary>
        public void StopScheduler()
        {
            if (_cancellationTokenSource == null)
            {
                throw new InvalidOperationException("Scheduler is not running.");
            }

            _cancellationTokenSource.Cancel();
            try
            {
                _schedulerTask.Wait();
            }
            catch (AggregateException ex)
            {
                // Handle any exceptions thrown during task cancellation.
                foreach (var inner in ex.InnerExceptions)
                {
                    Console.WriteLine($"Task cancellation exception: {inner.Message}");
                }
            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Executes the job logic.
        /// </summary>
        public static void ExecuteJob()
        {
            // Let's not run when there's no data present
            if (levels == null)
            {
                return;
            }

            // Get step of the job
            jobCounter += 1;
            if (jobCounter >= levels.Length)
            {
                jobCounter = 0;
            }

            // Simulate sending data to LevelBarGenerator
            LevelBarGenerator.Instance.ReceiveLevelData(channelIds, levels[jobCounter]);
        }

        private static byte[][] GenerateData(out float[][] levels, out int[] channelIds, out int[] numsRawData)
        {
            // random
            Random randomLevel = new Random();

            // Generate meta data
            numsRawData = Enumerable.Repeat(channelBlockSize, numberOfChannels).ToArray();
            channelIds = Enumerable.Range(0, numberOfChannels).ToArray();

            // Helpers
            int numberOfDataPointsPerChannel = 5 * (int)(samplingTime * samplingRate);
            int numberOfBlocks = numberOfDataPointsPerChannel / (channelBlockSize / 8);
            int numberOfTriggerBlocks = (int)(samplingTime * samplingRate) / (channelBlockSize / 8);

            // Generate the real data
            int triggerChannel = 36;
            byte[][] rawData = new byte[numberOfBlocks][];
            levels = new float[numberOfBlocks][];

            for (int b = 0; b < numberOfBlocks; b++)
            {
                List<byte> blockData = new List<byte>();
                List<float> blockLevels = new List<float>();

                for (int i = 0; i < numberOfChannels; i++)
                {
                    if (i != triggerChannel)
                    {
                        // Response Data
                        double[] data = new double[channelBlockSize / 8];
                        Normal normal = new Normal(0, 0.01);
                        normal.Samples(data);

                        // Trigger Data
                        if (b < numberOfTriggerBlocks)
                        {
                            float factor = (numberOfTriggerBlocks - b) / (float)numberOfTriggerBlocks;
                            factor = (float)Math.Exp(1 - (1 / (factor * factor)));
                            double[] sineData = Generate.Sinusoidal(channelBlockSize / 8, samplingRate, randomLevel.NextDouble() * 1000, factor);

                            for (int k = 0; k < data.Length; k++)
                            {
                                data[k] += sineData[k];
                            }
                        }

                        blockData.AddRange(GetBytes(data));
                        blockLevels.Add((float)(data.Select(c => Math.Abs(c)).Max() / 10d));
                    }
                    else
                    {
                        // Trigger Data
                        double[] data = new double[channelBlockSize / 8];
                        Normal normal = new Normal(0, 0.01);
                        normal.Samples(data);

                        if (b == 0)
                        {
                            double[] triggerData = Generate.Impulse(channelBlockSize / 8, 1, 50);
                            triggerData[49] = 0.9;
                            triggerData[48] = 0.7;
                            triggerData[47] = 0.2;
                            triggerData[46] = 0.1;
                            triggerData[51] = 0.9;
                            triggerData[52] = 0.7;
                            triggerData[53] = 0.2;
                            triggerData[54] = 0.1;

                            for (int k = 0; k < data.Length; k++)
                            {
                                data[k] += triggerData[k];
                            }
                        }

                        blockData.AddRange(GetBytes(data));
                        blockLevels.Add((float)(data.Select(c => Math.Abs(c)).Max() / 10d));
                    }
                }

                // Set the value
                rawData[b] = blockData.ToArray();
                levels[b] = blockLevels.ToArray();
            }

            return rawData;
        }

        private static byte[] GetBytes(double[] values)
        {
            var result = new byte[values.Length * sizeof(double)];
            Buffer.BlockCopy(values, 0, result, 0, result.Length);
            return result;
        }
    }
}
