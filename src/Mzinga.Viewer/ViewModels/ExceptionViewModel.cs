// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

namespace Mzinga.Viewer.ViewModels
{
    public class ExceptionViewModel : InformationViewModel
    {
        #region Properties

        public Exception Exception
        {
            get
            {
                return _exception;
            }
            private set
            {
                _exception = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
        private Exception _exception;

        #endregion

        public ExceptionViewModel(Exception exception) : base(exception?.Message, "Error", $"```{exception}```")
        {
            if (exception is EngineInvalidMoveException)
            {
                Title = "Invalid Move";
                Details = null;
            }
            else if (exception is EngineErrorException ee)
            {
                Title = "Engine Error";
                Details = $"```{string.Join(Environment.NewLine, ee.OutputLines)}```";
            }
        }
    }
}
