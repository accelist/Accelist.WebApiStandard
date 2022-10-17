using Accelist.WebApiStandard.Contracts.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accelist.WebApiStandard.Contracts.RequestModels
{
    public class TestEmptyRequest : IRequest<TestResponse>
    {
    }
}
