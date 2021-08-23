using System.Threading.Tasks;

namespace PBDialGauge.CommonHelpers
{
    public interface IColorValidationHelper
    {
        Task<bool> ValidateColor(Task<string[]> colors);
    }
}