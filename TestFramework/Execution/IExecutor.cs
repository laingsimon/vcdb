using System.Threading.Tasks;

namespace TestFramework.Execution
{
    internal interface ITestFramework
    {
        Task<int> Execute();
    }
}