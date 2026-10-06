using OrbitWatch.Models.Api;
using OrbitWatch.Models.Orbit;

namespace OrbitWatch.Services.Orbit
{
    /// <summary>
    /// Service for propagating satellite orbits from CelesTrak GP orbital elements.
    /// Calculates predicted satellite positions at specified UTC timestamps
    /// without requiring additional HTTP requests per calculation.
    /// </summary>
    public interface IOrbitPropagationService
    {
        /// <summary>
        /// Calculates the predicted position of a satellite at a given UTC time
        /// using SGP4 propagation from the provided CelesTrak GP orbital elements.
        /// </summary>
        /// <param name="gpData">CelesTrak GP data containing orbital elements.</param>
        /// <param name="utcTime">The UTC timestamp at which to calculate the position.</param>
        /// <returns>
        /// A <see cref="SatellitePosition"/> with the predicted position,
        /// or null if the orbital data is invalid or propagation fails.
        /// </returns>
        SatellitePosition? GetPosition(CelesTrakGpData gpData, DateTime utcTime);

        /// <summary>
        /// Calculates predicted positions of a satellite at multiple UTC timestamps
        /// using a single set of orbital elements (no additional HTTP requests).
        /// </summary>
        /// <param name="gpData">CelesTrak GP data containing orbital elements.</param>
        /// <param name="utcTimes">The UTC timestamps at which to calculate positions.</param>
        /// <returns>
        /// A list of <see cref="SatellitePosition"/> results for each valid timestamp.
        /// Timestamps that fail propagation are omitted from the results.
        /// </returns>
        IReadOnlyList<SatellitePosition> GetPositions(
            CelesTrakGpData gpData,
            IEnumerable<DateTime> utcTimes);
    }
}
