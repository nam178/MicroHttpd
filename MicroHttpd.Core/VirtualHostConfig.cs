using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MicroHttpd.Core
{
	public sealed class VirtualHostConfig : IVirtualHostConfigReadOnly
	{
		IStringMatch _hostName = new StringMatch.MatchAll();
		public IStringMatch HostName
		{
			get => _hostName;
			set
			{
				_hostName = value ?? throw new ArgumentNullException(nameof(value));
			}
		}

		int[] _listenOnPorts = new int[] { 8443 };
		public int[] ListenOnPorts
		{
			get => _listenOnPorts;
			set
			{
				if(null == value)
					throw new ArgumentNullException(nameof(value));
				if(value.Length == 0)
					throw new ArgumentException("Must specify at least one port");
				foreach(var port in value)
					Validation.RequireValidPort(port);
				_listenOnPorts = value;
			}
		}

		string _documentRoot = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
		public string DocumentRoot
		{
			get => _documentRoot;
			set
			{
				if(string.IsNullOrWhiteSpace(value))
					throw new ArgumentException(value);
				if(false == Directory.Exists(value))
					throw new DirectoryNotFoundException($"Directory not found {value}");
				_documentRoot = value;
			}
		}

		IReadOnlyList<int> IVirtualHostConfigReadOnly.ListenOnPorts
		{ get => _listenOnPorts; }
	}
}

