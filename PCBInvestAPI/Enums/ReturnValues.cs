using System;
using System.Collections.Generic;
using System.Text;

namespace PCBInvestAPI
{
    public enum ReturnValue
    {
        Successful_as_xlsx = 0,
        Successful_as_csv,
        PCBIsAvailable,
        PCBIsNotAvailable,
        withError,
        CouldnnotReadSettings,
        ConsoloutputJson

    }
}
