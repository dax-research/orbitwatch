/* Why JsonPropertyName?

CelesTrak uses:

"OBJECT_NAME"
"NORAD_CAT_ID"
"MEAN_MOTION"

but C# conventionally uses:

ObjectName
NoradCatalogId
MeanMotion

JsonPropertyName tells System.Text.Json:

"When JSON contains OBJECT_NAME, put it into ObjectName."
*/

using System.Text.Json.Serialization;

namespace OrbitWatch.Models.Api
{
    public class CelesTrakGpData
    {
        [JsonPropertyName("OBJECT_NAME")]
        public string? ObjectName { get; set; }

        [JsonPropertyName("OBJECT_ID")]
        public string? ObjectId { get; set; }

        [JsonPropertyName("EPOCH")]
        public DateTime? Epoch { get; set; }

        [JsonPropertyName("MEAN_MOTION")]
        public double? MeanMotion { get; set; }

        [JsonPropertyName("ECCENTRICITY")]
        public double? Eccentricity { get; set; }

        [JsonPropertyName("INCLINATION")]
        public double? Inclination { get; set; }

        [JsonPropertyName("RA_OF_ASC_NODE")]
        public double? RightAscensionOfAscendingNode { get; set; }

        [JsonPropertyName("ARG_OF_PERICENTER")]
        public double? ArgumentOfPericenter { get; set; }

        [JsonPropertyName("MEAN_ANOMALY")]
        public double? MeanAnomaly { get; set; }

        [JsonPropertyName("EPHEMERIS_TYPE")]
        public int? EphemerisType { get; set; }

        [JsonPropertyName("CLASSIFICATION_TYPE")]
        public string? ClassificationType { get; set; }

        [JsonPropertyName("NORAD_CAT_ID")]
        public int? NoradCatalogId { get; set; }

        [JsonPropertyName("ELEMENT_SET_NO")]
        public int? ElementSetNumber { get; set; }

        [JsonPropertyName("REV_AT_EPOCH")]
        public int? RevolutionNumberAtEpoch { get; set; }

        [JsonPropertyName("BSTAR")]
        public double? BStar { get; set; }

        [JsonPropertyName("MEAN_MOTION_DOT")]
        public double? MeanMotionDot { get; set; }

        [JsonPropertyName("MEAN_MOTION_DDOT")]
        public double? MeanMotionDdot { get; set; }
    }
}