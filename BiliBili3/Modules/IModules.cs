using BiliBili3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliBili3.Modules
{
    public interface IModules
    {
        ReturnModel HandelError(Exception ex);
        ReturnModel<T> HandelError<T>(Exception ex);
    }
}
