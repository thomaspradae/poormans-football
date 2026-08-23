using NUnit.Framework;
using UnityEngine;

namespace PoormansFootball.Tests.EditMode
{
    internal sealed class ProjectBootstrapTests
    {
        [Test]
        public void UnityRuntimeIsAvailable()
        {
            Assert.That(Application.unityVersion, Does.StartWith("6000.5"));
        }

        [Test]
        public void RegulationPitchDimensionsRemainCanonical()
        {
            const float pitchLengthMetres = 105f;
            const float pitchWidthMetres = 68f;

            Assert.That(pitchLengthMetres, Is.GreaterThan(pitchWidthMetres));
            Assert.That(pitchLengthMetres * pitchWidthMetres, Is.EqualTo(7140f));
        }
    }
}
