// <copyright file="MainWindowViewModel.cs" company="VIBES.technology">
// Copyright (c) VIBES.technology. All rights reserved.
// </copyright>

namespace LevelBarApp.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;
    using LevelBarGeneration;

    /// <summary>
    /// MainWindowViewModel
    /// </summary>
    /// <seealso cref="ViewModelBase" />
    public class MainWindowViewModel : ViewModelBase
    {
        // Fields
        private readonly LevelBarGenerator levelBarGenerator;
        private RelayCommand connectToGeneratorCommand;
        private RelayCommand disconnectToGeneratorCommand;
        private DateTime lastUpdateTime = DateTime.MinValue;
        private readonly TimeSpan updateInterval = TimeSpan.FromSeconds(0.3);

        // Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        public MainWindowViewModel()
        {
            levelBarGenerator = LevelBarGenerator.Instance;

            levelBarGenerator.GeneratorStateChanged += LevelBarGenerator_GeneratorStateChanged;
            levelBarGenerator.ChannelAdded += LevelBarGenerator_ChannelAdded;
            levelBarGenerator.ChannelLevelDataReceived += LevelBarGenerator_ChannelDataReceived;
            levelBarGenerator.ChannelRemoved += LevelBarGenerator_ChannelRemoved;
        }

        // Properties

        /// <summary>
        /// Gets or sets the level bars, one for each channel.
        /// </summary>
        /// <value>
        /// The level bars.
        /// </value>
        public ObservableCollection<LevelBarViewModel> LevelBars { get; set; } = new ObservableCollection<LevelBarViewModel>();

        /// <summary>
        /// Gets the command to connect the generator
        /// </summary>
        /// <value>
        /// The connect generator.
        /// </value>
        public RelayCommand ConnectGeneratorCommand => connectToGeneratorCommand ?? (connectToGeneratorCommand = new RelayCommand((async () => await levelBarGenerator.Connect())));



        /// <summary>
        /// Gets the command to disconnect the generator
        /// </summary>
        /// <value>
        /// The disconnect generator.
        /// </value>
        public RelayCommand DisconnectGeneratorCommand => disconnectToGeneratorCommand ?? (disconnectToGeneratorCommand = new RelayCommand((async () => await levelBarGenerator.Disconnect())));

        // Methods
        private void LevelBarGenerator_ChannelAdded(object sender, ChannelChangedEventArgs e)
        {
            try
            {
                LevelBarViewModel levelBarVM = new LevelBarViewModel() { Id = e.ChannelId, Name = "Channel " + e.ChannelId.ToString() };
                LevelBars.Add(levelBarVM);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error occured while adding Channel {0}.\n{1}", e.ChannelId, ex.Message));
            }
        }

        private void LevelBarGenerator_ChannelRemoved(object sender, ChannelChangedEventArgs e)
        {
            // Remove the corresponding LevelBarViewModel
            try
            {
                LevelBars.Remove(LevelBars.FirstOrDefault(lb => lb.Id == e.ChannelId));
            }
            catch(Exception ex)
            {
                MessageBox.Show(string.Format("Error occured while removing Channel {0}.\n{1}", e.ChannelId, ex.Message));
            }           
        }

        private void LevelBarGenerator_GeneratorStateChanged(object sender, GeneratorStateChangedEventArgs e)
        {
            // TODO Set the state of the generator
        }

        private void LevelBarGenerator_ChannelDataReceived(object sender, ChannelDataEventArgs e)
        {
            // TODO this is where the level data is coming in
            try
            {
                // Throttle the updates
                if ((DateTime.Now - lastUpdateTime) < updateInterval) return;

                lastUpdateTime = DateTime.Now;

                UpdateClusterBounds(e.Levels);
                for (int i = 0; i < e.ChannelIds.Length; i++)
                {
                    var itemToUpdate = LevelBars.FirstOrDefault(lb => lb.Id == e.ChannelIds[i]);
                    if (itemToUpdate != null)
                    {
                        //float scaledLevel = (float)Math.Pow(e.Levels[i], 0.5); // Power scaling
                        float scaledLevel = (float)ScaledLevel(e.Levels[i]);
                        itemToUpdate.Level = scaledLevel;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error occured while receiving channel data.\n{0}", ex.Message));
            }
        }
        //Linear Scaling with Dynamic Range Adjustment
        private double MinCluster = 0.098;
        private double MaxCluster = 0.101;

        //public double ScaledLevel(double level)
        //{
        //    // Clamp level to the observed range
        //    double clampedLevel = Math.Max(MinCluster, Math.Min(MaxCluster, level));

        //    // Scale linearly to 0.0 - 1.0
        //    return (clampedLevel - MinCluster) / (MaxCluster - MinCluster);
        //}

        public double ScaledLevel(double level)
        {
            double clampedLevel = Math.Max(MinCluster, Math.Min(MaxCluster, level));

            double normalizedLevel = (clampedLevel - MinCluster) / (MaxCluster - MinCluster);

            // Ensure the normalized level is above zero for logarithmic conversion
            double safeLevel = Math.Max(normalizedLevel, 1e-9);

            // Convert to dB scale (VU meters typically use a dB range)
            double dbLevel = 20 * Math.Log10(safeLevel);

            double minDb = -60.0;
            double maxDb = 0.0;
            return (dbLevel - minDb) / (maxDb - minDb);
        }

        private void UpdateClusterBounds(float[] recentLevels)
        {
            if (recentLevels.Length == 0) return;

            MinCluster = recentLevels.Min();
            MaxCluster = recentLevels.Max();
        }
    }
}
