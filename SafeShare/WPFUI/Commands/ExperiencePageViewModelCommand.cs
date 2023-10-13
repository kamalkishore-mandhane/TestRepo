using SafeShare.WPFUI.ViewModel;
using System.Reflection;

namespace SafeShare.WPFUI.Commands
{
    public class ExperiencePageViewModelCommand
    {
        private ExperiencePageViewModel _viewModel;

        private ModelCommand _skipDoneCommand;
        private ModelCommand _submitCommand;
        private ModelCommand _surveyCommand;
        private ModelCommand _policyLinkCommand;

        public ExperiencePageViewModelCommand(ExperiencePageViewModel viewModel)
        {
            _viewModel = viewModel;

            _skipDoneCommand = new ModelCommand(ExecuteSkipDoneCommand, CanExecuteSkipDoneCommand);
            _submitCommand = new ModelCommand(ExecuteSubmitCommand, CanExecuteSubmitCommand);
            _surveyCommand = new ModelCommand(ExecuteSurveyCommand, CanExecuteSurveyCommand);
            _policyLinkCommand = new ModelCommand(ExecutePolicyLinkCommand, CanExecutePolicyLinkCommand);
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand SkipDoneCommand
        {
            get { return _skipDoneCommand; }
        }

        private bool CanExecuteSkipDoneCommand(object parameter)
        {
            return true;
        }

        private void ExecuteSkipDoneCommand(object parameter)
        {
            _viewModel.ExecuteSkipDoneCommand();
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand SubmitCommand
        {
            get { return _submitCommand; }
        }

        private bool CanExecuteSubmitCommand(object parameter)
        {
            return true;
        }

        private void ExecuteSubmitCommand(object parameter)
        {
            _viewModel.ExecuteSubmitCommand();
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand SurveyCommand
        {
            get { return _surveyCommand; }
        }

        private bool CanExecuteSurveyCommand(object parameter)
        {
            return true;
        }

        private void ExecuteSurveyCommand(object parameter)
        {
            _viewModel.ExecuteSurveyCommand();
        }

        [Obfuscation(Exclude = true)]
        public ModelCommand PolicyLinkCommand
        {
            get { return _policyLinkCommand; }
        }

        private bool CanExecutePolicyLinkCommand(object parameter)
        {
            return true;
        }

        private void ExecutePolicyLinkCommand(object parameter)
        {
            _viewModel.ExecutePolicyLinkCommand();
        }
    }
}