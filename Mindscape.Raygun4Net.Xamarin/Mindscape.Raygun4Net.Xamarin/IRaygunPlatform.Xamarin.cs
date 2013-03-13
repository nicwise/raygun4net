using System;
using Mindscape.Raygun4Net.Messages;

namespace Mindscape.Raygun4Net
{
	public interface IRaygunPlatform
	{
		void Ingest(RaygunMessageDetails details);
	}
}

