﻿// <copyright file="LevelBarGenerator.cs" company="VIBES.technology">
// Copyright (c) VIBES.technology. All rights reserved.
// </copyright>

namespace LevelBarGeneration
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// LevelBarGenerator
    /// </summary>
    public class LevelBarGenerator
    {
        // Fields
        private Timer _timer;
        private GeneratorState state = GeneratorState.Stopped;
        private CancellationTokenSource _cancellationTokenSource;

        // Constructor

        /// <summary>
        /// Prevents a default instance of the <see cref="LevelBarGenerator"/> class from being created.
        /// </summary>
        private LevelBarGenerator()
        {

        }

        // Events

        /// <summary>
        /// Occurs when [channel added].
        /// </summary>
        public event EventHandler<ChannelChangedEventArgs> ChannelAdded;

        /// <summary>
        /// Occurs when [channel removed].
        /// </summary>
        public event EventHandler<ChannelChangedEventArgs> ChannelRemoved;

        /// <summary>
        /// Occurs when [channel data received].
        /// </summary>
        public event EventHandler<ChannelDataEventArgs> ChannelLevelDataReceived;

        /// <summary>
        /// Occurs when [state changed].
        /// </summary>
        public event EventHandler<GeneratorStateChangedEventArgs> GeneratorStateChanged;

        // Properties

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static LevelBarGenerator Instance { get; } = new LevelBarGenerator();

        // Methods

        /// <summary>
        /// Connects this instance.
        /// </summary>
        /// <returns>Connect Task</returns>
        public async Task Connect()
        {
            if (state == GeneratorState.Running)
            {
                Console.WriteLine("Generator is already connected");
                return;
            }

            // What properties are used
            int channelBlockSize = 512;
            int samplingRate = 16384;
            double samplingTime = 1.0d;

            // Setup the channels
            int numberOfChannels = RegisterChannels();

            // Setup and fire the data generator
            await SetupDataGenerator(channelBlockSize, samplingRate, samplingTime, numberOfChannels);

            state = GeneratorState.Running;
            GeneratorStateChanged?.Invoke(this, new GeneratorStateChangedEventArgs { State = GeneratorState.Running });
        }

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        /// <returns>Disconnect Task</returns>
        public async Task Disconnect()
        {
            StopDataGenerator();

            DeregisterChannels();

            state = GeneratorState.Stopped;
            GeneratorStateChanged?.Invoke(this, new GeneratorStateChangedEventArgs { State = GeneratorState.Stopped });

            await Task.CompletedTask;
        }

        /// <summary>
        /// Receives the level data.
        /// </summary>
        /// <param name="channelIds">The channel ids.</param>
        /// <param name="levels">The levels.</param>
        internal void ReceiveLevelData(int[] channelIds, float[] levels)
        {
            ChannelLevelDataReceived?.Invoke(this, new ChannelDataEventArgs { ChannelIds = channelIds, Levels = levels });
        }

        private async Task SetupDataGenerator(int channelBlockSize, int samplingRate, double samplingTime, int numberOfChannels)
        {
            DataThroughputJob.SetupJob(samplingRate, channelBlockSize, samplingTime, numberOfChannels);

            // Calculate the interval
            double intervalMilliseconds = (channelBlockSize / 8d) / samplingRate * 1000d;

            // Initialize the timer
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            _ = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        DataThroughputJob.ExecuteJob(); // Execute the job logic
                        await Task.Delay((int)intervalMilliseconds, token);
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

            await Task.CompletedTask;
        }

        private void StopDataGenerator()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private int RegisterChannels()
        {
            int numberOfChannels = 75;
            for (int i = 0; i < numberOfChannels; ++i)
            {
                ChannelAdded?.Invoke(this, new ChannelChangedEventArgs { ChannelId = i });
            }

            return numberOfChannels;
        }

        private void DeregisterChannels()
        {
            int numberOfChannels = 75;
            for (int i = 0; i < numberOfChannels; ++i)
            {
                ChannelRemoved?.Invoke(this, new ChannelChangedEventArgs { ChannelId = i });
            }
        }
    }
}
