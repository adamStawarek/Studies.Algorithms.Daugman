using System;

namespace ImageEditor.Filters.Interfaces
{
    public interface IError
    {
        string ErrorMessage { get; set; }
        event EventHandler ErrorOccured;
        event EventHandler NoErrorOccured;
        void OnSuccess();
        void OnError(string error);
    }
}
