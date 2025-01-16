// <copyright file="MainWindowViewModel.cs" company="VIBES.technology">
// Copyright (c) VIBES.technology. All rights reserved.
// </copyright>

namespace LevelBarApp.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO.Packaging;
    using System.Linq;
    using System.Threading.Tasks;
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
        public RelayCommand ConnectGeneratorCommand => connectToGeneratorCommand ?? (connectToGeneratorCommand = new RelayCommand(new System.Action(async () => await levelBarGenerator.Connect())));

        /// <summary>
        /// Gets the command to disconnect the generator
        /// </summary>
        /// <value>
        /// The disconnect generator.
        /// </value>
        public RelayCommand DisconnectGeneratorCommand => disconnectToGeneratorCommand ?? (disconnectToGeneratorCommand = new RelayCommand(new System.Action(async () => await levelBarGenerator.Disconnect())));

        // Methods
        private void LevelBarGenerator_ChannelAdded(object sender, ChannelChangedEventArgs e)
        {
            // Generate a LevelBarViewModel
            //if (LevelBars.Count==0)
            //{
            //    StartResetProcess();
            //}

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
                for (int i = 0; i < e.ChannelIds.Length; i++)
                {
                    var itemToUpdate = LevelBars.FirstOrDefault(lb => lb.Id == e.ChannelIds[i]);
                    if (itemToUpdate != null)
                    {
                        //float logScaledLevel = (float)Math.Log10(e.Levels[i] + 1); // Scale 0 to 1 logarithmically
                        float scaledLevel = (float)Math.Pow(e.Levels[i], 0.5); // Power scaling
                        //itemToUpdate.MaxLevel = Math.Max(itemToUpdate.MaxLevel, scaledLevel);
                        itemToUpdate.Level = scaledLevel;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error occured while receiving channel data.\n{0}", ex.Message));
            }

           
        }

        private async void StartResetProcess()
        {
            while (true)
            {
                await Task.Delay(2000);  // Wait for 2 seconds

                // Reset MaxLevel and Level values
                foreach (var item in LevelBars)
                {
                    item.MaxLevel = 0;  // Reset to 0 or any other default value
                }
            }
        }
    }
}
