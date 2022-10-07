﻿using Reloaded.Mod.Launcher.Pages.Dialogs.InstallModPackPages;

namespace Reloaded.Mod.Launcher.Pages.Dialogs;

/// <summary>
/// Interaction logic for InstallModPackDialog.xaml
/// </summary>
public partial class InstallModPackDialog : ReloadedWindow
{
    public new InstallModPackDialogViewModel ViewModel { get; }

    public InstallModPackDialog(InstallModPackDialogViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        InitInitialPage();
        ViewModel.PropertyChanged += OnPageIndexPropertyChanged;
    }

    private void OnPageIndexPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ViewModel.PageIndex))
            return;

        var index = ViewModel.PageIndex;
        if (index == 0)
        {
            InitInitialPage();
            return;
        }

        var itemOffset = index - 1;
        if (itemOffset < ViewModel.Mods.Count)
        {
            InitForMod(itemOffset);
            return;
        }

        // Init download page.
        PageHost.CurrentPage = new InstallModDownloadPage(ViewModel);
    }

    private void InitForMod(int itemOffset)
    {
        PageHost.CurrentPage = new InstallModPackModPage(new InstallModPackModPageViewModel(ViewModel, ViewModel.Mods[itemOffset]));
    }

    private void InitInitialPage()
    {
        PageHost.CurrentPage = new InstallModPackEntryPage(new InstallModPackEntryPageViewModel(ViewModel));
    }
}