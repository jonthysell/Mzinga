// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

namespace Mzinga.Viewer.ViewModels
{
    public class ExceptionViewModel : InformationViewModel
    {
        #region Properties

        public override bool ShowDetails => !(Exception is EngineInvalidMoveException);

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

        public ExceptionViewModel(Exception exception) : base(exception?.Message, exception is EngineInvalidMoveException ? "Invalid Move" : exception is EngineErrorException ? "Engine Error" : "Error", $"```{ (exception is EngineErrorException ee ? string.Join(Environment.NewLine, ee.OutputLines) : exception.ToString()) }```")
        {
            Exception = exception;
        }
    }
}
