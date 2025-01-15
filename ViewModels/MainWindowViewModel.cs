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



            ///test
            ///
            //LevelBars = new ObservableCollection<LevelBarViewModel>
            //{
            //    new LevelBarViewModel { Name = "Channel 1", Level = 0.3f },
            //    new LevelBarViewModel { Name = "Channel 2", Level = 0.7f },
            //    new LevelBarViewModel { Name = "Channel 3", Level = 0.9f }
            //};

            //Task.Run(async () =>
            //{
            //    while (true)
            //    {
            //        await Task.Delay(1000);
            //        foreach (var bar in LevelBars)
            //        {
            //            bar.Level = new Random().Next(0, 101);
            //        }
            //    }
            //});
            ///test end
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
                        itemToUpdate.Level = e.Levels[i];
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error occured while receiving channel data.\n{0}", ex.Message));
            }

           
        }
    }
}
