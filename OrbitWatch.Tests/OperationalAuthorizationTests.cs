using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Xunit;
using OrbitWatch.Controllers;

namespace OrbitWatch.Tests
{
    public class OperationalAuthorizationTests
    {
        private static readonly Type[] OperationalControllers = new[]
        {
            typeof(SatellitesController),
            typeof(MissionsController),
            typeof(GroundStationsController),
            typeof(TrajectoryRecordsController),
            typeof(IncidentsController),
            typeof(SatelliteObservationsController)
        };

        [Fact]
        public void OperationalControllers_RequireAuthentication_By_Default()
        {
            foreach (var controller in OperationalControllers)
            {
                var authorizeAttr = controller.GetCustomAttribute<AuthorizeAttribute>();
                Assert.NotNull(authorizeAttr);
                Assert.Null(authorizeAttr.Roles); // Should be generic [Authorize]
            }
        }

        [Fact]
        public void Analyst_Cannot_Mutate_Data_Manager_And_Admin_Can()
        {
            foreach (var controller in OperationalControllers)
            {
                var mutatingMethods = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(m => m.Name == "Create" || m.Name == "Edit" || m.Name == "Delete" || m.Name == "DeleteConfirmed");

                Assert.NotEmpty(mutatingMethods);

                foreach (var method in mutatingMethods)
                {
                    var authAttrs = method.GetCustomAttributes<AuthorizeAttribute>(true).ToList();
                    Assert.NotEmpty(authAttrs);
                    
                    var roleAuth = authAttrs.FirstOrDefault(a => !string.IsNullOrEmpty(a.Roles));
                    Assert.NotNull(roleAuth);
                    
                    var roles = roleAuth.Roles.Split(',').Select(r => r.Trim()).ToList();
                    
                    // Analyst cannot Edit/Create/Delete
                    Assert.DoesNotContain("Analyst", roles);
                    
                    // Admin and Manager can
                    Assert.Contains("Admin", roles);
                    Assert.Contains("Manager", roles);
                }
            }
        }

        [Fact]
        public void Tracking_And_OrbitApi_Remain_Accessible()
        {
            // TrackingController
            var trackingAuth = typeof(TrackingController).GetCustomAttribute<AuthorizeAttribute>();
            Assert.NotNull(trackingAuth);
            Assert.Null(trackingAuth.Roles); // Accessible to all authenticated users

            // OrbitController API (Anonymous)
            var orbitAuth = typeof(Controllers.Api.OrbitController).GetCustomAttribute<AuthorizeAttribute>();
            Assert.Null(orbitAuth);
            
            var getPositionMethod = typeof(Controllers.Api.OrbitController).GetMethod("GetPosition");
            Assert.NotNull(getPositionMethod);
            Assert.Empty(getPositionMethod.GetCustomAttributes<AuthorizeAttribute>());
        }
    }
}
