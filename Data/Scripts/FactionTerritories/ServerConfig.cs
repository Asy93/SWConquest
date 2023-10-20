using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using VRage.Library;
using VRage;
using VRage.Serialization;
using Sandbox.ModAPI;
using System.IO;
using Sandbox.Engine.Utils;
using System.Xml.Serialization;

namespace Faction_Territories.Config
{
	[Serializable]
	[ProtoContract]
	public class ServerConfig
	{
		[ProtoMember(1)]
		public SerializableDictionary<long, string> AllianceNames;

		[ProtoMember(2)]
		public SerializableDictionary<long, ulong> AllianceChannelIds;

		[ProtoMember(3)]
		public ulong GlobalDiscordChannelId;


		public ServerConfig()
		{
			AllianceNames = new SerializableDictionary<long, string>(new Dictionary<long, string> { { 0, "None" } });
			AllianceChannelIds = new SerializableDictionary<long, ulong>(new Dictionary<long, ulong> { { 0, 0 } });
		}

		public ServerConfig(List<string> names, List<ulong> ids, ulong globalId)
		{
			Dictionary<long, string> dictNames = AllianceNames.Dictionary;
			Dictionary<long, ulong> dictIds = AllianceChannelIds.Dictionary;

			if (names.Count == ids.Count)
			{
				for (int i = dictNames.Count; i < names.Count; i++)
				{
					dictNames[i] = names[i - 1];
					dictIds[i] = ids[i - 1];
				}
			}

			AllianceNames = new SerializableDictionary<long, string>(dictNames);
			AllianceChannelIds = new SerializableDictionary<long, ulong>(dictIds);
			GlobalDiscordChannelId = globalId;
		}

		public Dictionary<long, ulong> Setup(List<string> allianceNames)
		{
			if (AllianceNames?.Dictionary?.Values != null && AllianceNames.Dictionary.Count == AllianceChannelIds.Dictionary.Count)
			{
				allianceNames.Clear();
				foreach (string name in AllianceNames?.Dictionary?.Values)
					allianceNames.Add(name);
				return AllianceChannelIds.Dictionary;
			}
			allianceNames = new List<string> { "None" };
			return new Dictionary<long, ulong> { { 0, 0 } };
		}

		public static ServerConfig Load()
		{
			ServerConfig config = new ServerConfig();
			if (MyAPIGateway.Utilities.FileExistsInWorldStorage("config.xml", typeof(Session)))
			{
				try
				{
					using (TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage("config.xml", typeof(ServerConfig)))
					{
						string data = reader.ReadToEnd();
						config = MyAPIGateway.Utilities.SerializeFromXML<ServerConfig>(data);
					}
				}
				catch { return new ServerConfig(); }
			}

			if (config == null)
				config = new ServerConfig();

			return config;
		}

		public void Save()
		{
			try
			{
				using (TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage("config.xml", typeof(ServerConfig)))
				{
					string data = MyAPIGateway.Utilities.SerializeToXML<ServerConfig>(this);
					if (data != null)
						writer.Write(data);
				}
			}
			catch { return ; }
		}
	}
}
