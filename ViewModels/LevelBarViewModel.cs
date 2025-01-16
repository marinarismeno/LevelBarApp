// <copyright file="LevelBarViewModel.cs" company="VIBES.technology">
// Copyright (c) VIBES.technology. All rights reserved.
// </copyright>

namespace LevelBarApp.ViewModels
{
    using System;
    using System.Windows;
    using System.Windows.Threading;
    using GalaSoft.MvvmLight;

    /// <summary>
    /// Represents a level bar for a channel
    /// </summary>
    /// <seealso cref="ViewModelBase" />
    public class LevelBarViewModel : ViewModelBase
    {
        // Fields
        private string name = string.Empty;
        private float level = 0.0f;
        private float maxLevel = 0.0f;
        private int id;
        private float lastMaxLevel = 0.0f; // Peak hold value
        private DateTime lastResetTime = DateTime.Now;
        private DispatcherTimer resetTimer;


        // Properties

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id
        {
            get => id;
            set
            {
                id = value;
                RaisePropertyChanged(nameof(Id));
            }
        }

        /// <summary>
        /// Gets or sets the name of the channel.
        /// </summary>
        /// <value>
        /// The name of the channel.
        /// </value>
        public string Name
        {
            get => name;
            set
            {
                name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// Gets or sets the level.
        /// </summary>
        /// <value>
        /// The level.
        /// </value>
        public float Level
        {
            get => level;
            set
            {
                level = value;
                UpdateMaxLevel();
                RaisePropertyChanged(nameof(Level));
            }
        }

        /// <summary>
        /// Gets or sets the maximum level used of the peakhold.
        /// </summary>
        /// <value>
        /// The maximum level.
        /// </value>
        public float MaxLevel
        {
            get => maxLevel;
            set
            {
                maxLevel = value;
                RaisePropertyChanged(nameof(MaxLevel));
            }
        }
        
        public float LastMaxLevel
        {
            get => lastMaxLevel;
            set
            {
                lastMaxLevel = value;
                RaisePropertyChanged(nameof(LastMaxLevel));
            }
        }
        
        public DateTime LastResetTime
        {
            get => lastResetTime;
            set
            {
                lastResetTime = value;
                RaisePropertyChanged(nameof(LastResetTime));
            }
        }
        
        public DispatcherTimer ResetTimer
        {
            get => resetTimer;
            set
            {
                resetTimer = value;
                RaisePropertyChanged(nameof(ResetTimer));
            }
        }

        public LevelBarViewModel()
        {
            // Initialize the reset timer
            resetTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            resetTimer.Tick += ResetMaxLevel;
        }

        private void UpdateMaxLevel()
        {
            // Update the maximum level
            if (level > MaxLevel)
            {
                MaxLevel = level;

                // Restart the timer
                StartResetTimer();
            }
        }

        private void StartResetTimer()
        {
            // Stop the timer if it's already running
            if (resetTimer.IsEnabled)
            {
                resetTimer.Stop();
            }

            // Start the timer
            resetTimer.Start();
        }

        private void ResetMaxLevel(object sender, EventArgs e)
        {
            // Reset MaxLevel to the current Level
            MaxLevel = Level;

            // Stop the timer after resetting
            resetTimer.Stop();
        }
    }
}