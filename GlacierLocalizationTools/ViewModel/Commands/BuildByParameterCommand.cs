namespace GlacierLocalizationTools.ViewModel.Commands
{
    internal class BuildByParameterCommand : AbstractParameterCommand
    {
        protected override void DoSpecificWork()
        {
            myOneTimeRunViewModel.BuildPatch();
        }
    }
}
