// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

namespace Mzinga.Viewer.ViewModels
{
    public class ExceptionViewModel : InformationViewModel
    {
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

        public ExceptionViewModel(Exception exception) : base(exception?.Message, exception is EngineInvalidMoveException ? "Invalid Move" : "Error")
        {
            Exception = exception;
            Details = Exception is EngineErrorException ee ? string.Format(string.Join(Environment.NewLine, ee.OutputLines)) : string.Format(Exception.ToString());
        }
    }
}
