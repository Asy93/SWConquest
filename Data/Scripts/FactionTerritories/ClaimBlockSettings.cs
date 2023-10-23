using ProtoBuf;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Definitions;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.Components;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.ModAPI;
using VRageMath;
using Faction_Territories.Network;

namespace Faction_Territories
{
    public enum UIControls
    {
        Misc,
        Claiming,
        Sieging,
        Perks,
        TerritoryOptions
    }

    public enum PerkTypeList
    {
        Production
    }

    public enum PlayerPerks
    {
        Production
    }

    public enum TerritoryStatus
    {
        Offline,
        Neutral,
        Claimed,
        InitialSieging,
        AwaitingFinalSiege,
        FinalSieging,
        FailedSiegeCooldown,
        SuccessfulSiegeCooldown
    }

    [ProtoContract(IgnoreListHandling = true)]
    public class ClaimBlockSettings
    {
        public static readonly string SettingsVersion = "1.00";

        [ProtoMember(1)]
        public long _entityId;

        [ProtoMember(2)]
        public float _safeZoneSize;

        [ProtoMember(3)]
        public float _claimRadius;

        //[ProtoMember(4)]
        //public bool Sync;

        [ProtoMember(5)]
        public Vector3D _blockPos;

        [ProtoMember(6)]
        public string _claimedFaction;

        [ProtoMember(7)]
        public long _safeZoneEntity;

        [ProtoMember(8)]
        public bool _enabled;

        [ProtoMember(9)]
        public bool _isClaimed;

        [ProtoMember(10)]
        public int _toClaimTimer;

        //[ProtoIgnore]
        //public Dictionary<long, PlayerData> _playersInside;

        [ProtoMember(11)]
        public string _unclaimName;

        [ProtoMember(12)]
        public string _claimZoneName;

        [ProtoMember(13)]
        public List<long> _safeZones;

        [ProtoMember(14)]
        public int _timer;

        [ProtoMember(15)]
        public bool _isClaiming;

        [ProtoMember(16)]
        public double _distanceToClaim;

        [ProtoMember(17)]
        public string _detailInfo;

        [ProtoMember(18)]
        public long _jdClaimingId;

        [ProtoMember(19)]
        public long _playerClaimingId;

        [ProtoMember(20)]
        public int _recoverTimer;

        [ProtoMember(21)]
        public List<ZonesDelayRemove> _zonesDelay;

        [ProtoMember(22)]
        public int _consumeTokenTimer;

        [ProtoMember(23)]
        public int _toSiegeTimer;

        [ProtoMember(24)]
        public int _tokensToClaim;

        [ProtoMember(25)]
        public int _tokensToSiege;

        [ProtoMember(26)]
        public bool _isSieging;

        [ProtoMember(27)]
        public bool _isSieged;

        [ProtoMember(28)]
        public int _zoneDeactivationTimer;

        [ProtoMember(29)]
        public int _gpsUpdateDelay;

        [ProtoMember(30)]
        public double _distanceToSiege;

        [ProtoMember(31)]
        public bool _triggerInit;

        [ProtoMember(32)]
        public long _playerSiegingId;

        [ProtoMember(33)]
        public long _jdSiegingId;

        [ProtoMember(34)]
        public int _siegeTimer;

        [ProtoMember(35)]
        public ulong _discordChannelId;

        [ProtoMember(36)]
        public string _blockOwner;

        [ProtoMember(37)]
        public int _siegeNoficationFreq;

        [ProtoMember(38)]
        public long _factionId;

        [ProtoMember(39)]
        public EmissiveState _emissiveState;

        [ProtoMember(40)]
        public Dictionary<PerkType, PerkBase> _perks;

        [ProtoMember(41)]
        public UIControls _uiControls;

        [ProtoMember(42)]
        public PerkTypeList _uiPerkList;

        [ProtoMember(43)]
        public PlayerPerks _uiPlayerPerkList;

        [ProtoMember(44)]
        public bool _isSiegingFinal;

        [ProtoMember(45)]
        public bool _isSiegedFinal;

        [ProtoMember(46)]
        public string _version;

        [ProtoMember(47)]
        public bool _isCooling;

        [ProtoMember(48)]
        public int _toSiegeFinalTimer;

        [ProtoMember(49)]
        public int _tokensToSiegeFinal;

        [ProtoMember(50)]
        public int _tokensToDelaySiege;

        [ProtoMember(51)]
        public int _siegeDelayed;

        [ProtoMember(52)]
        public int _timeToDelay;

        [ProtoMember(53)]
        public int _siegeDelayAllowed;

        [ProtoMember(54)]
        public int _cooldownTime;

        [ProtoMember(55)]
        public string _siegedBy;

        [ProtoMember(56)]
        public bool _readyToSiege;

        [ProtoMember(57)]
        public int _timeframeToSiege;

        [ProtoMember(58)]
        public bool _centerToPlanet;

        [ProtoMember(59)]
        public Vector3D _planetCenter;

        [ProtoMember(60)]
        public bool _adminAllowSafeZoneAllies;

        [ProtoMember(61)]
        public bool _adminAllowTerritoryAllies;

        [ProtoMember(62)]
        public bool _allowSafeZoneAllies;

        [ProtoMember(63)]
        public bool _allowTerritoryAllies;

        [ProtoMember(64)]
        public bool _allowTools;

        [ProtoMember(65)]
        public bool _allowDrilling;

        [ProtoMember(66)]
        public bool _allowWelding;

        [ProtoMember(67)]
        public bool _allowGrinding;

        [ProtoMember(68)]
        public string _planetName;

        [ProtoMember(69)]
        public string _consumptionItem;

        [ProtoMember(70)]
        public int _consumptionAmt;

        [ProtoMember(71)]
        public bool _isSiegeCooling;

        [ProtoMember(72)]
        public int _siegeCoolingTime;

        [ProtoMember(73)]
        public TerritoryStatus _territoryStatus;

        [ProtoIgnore]
        public ServerData _server;

        //[ProtoIgnore]
        //public IMyEntity _jdSieging;

        //[ProtoIgnore]
        //public IMyEntity _jdClaiming;

        //[ProtoIgnore]
        //public IMyPlayer _playerClaiming;

        //[ProtoIgnore]
        //public IMyTerminalBlock _block;

        //[ProtoIgnore]
        //public List<GpsData> _gpsData;

        //[ProtoIgnore]
        //public Dictionary<long, GridData> _gridsInside;

        [ProtoMember(74)]
        public MySafeZoneAccess _accessTypeFactions;


        [ProtoMember(75)]
        public MySafeZoneAccess _accessTypeFloatingObjects;


        [ProtoMember(76)]
        public MySafeZoneAccess _accessTypeGrids;


        [ProtoMember(77)]
        public MySafeZoneAccess _accessTypePlayers;

        [ProtoMember(78)]
        public List<long> _factions;

        [ProtoMember(79)]
        public bool _visibleSZ;

		[ProtoMember(80)]
		public List<long> _factionsRadar;

		[ProtoMember(81)]
		public List<long> _factionsExempt;

		[ProtoMember(82)]
        public long _szTexture;

        [ProtoMember(83)]
        public Color _szColor;

        [ProtoMember(84)]
        public bool _neutralEnemies;

        [ProtoMember(85)]
        public DateTime? _finalSiegeDateTime;

        [ProtoMember(86)]
        public ulong _discordGlobalChannelId;

		[ProtoMember(87)]
		public long _allianceId;

		[ProtoMember(88)]
        public long _previousOwnerId;


		public ClaimBlockSettings()
        {
            _entityId = 0;
            _safeZoneSize = 0;
            _claimRadius = 0;
            //Sync = false;
            _blockPos = new Vector3D();
            _claimedFaction = "";
            _safeZoneEntity = 0;
            _enabled = false;
            _isClaimed = false;
            _toClaimTimer = 300;
            //_playersInside = new Dictionary<long, PlayerData>();
            _unclaimName = "";
            _claimZoneName = _unclaimName;
            _safeZones = new List<long>();
            _timer = 0;
            _isClaiming = false;
            _distanceToClaim = 0;
            _detailInfo = "";
            _jdClaimingId = 0;
            _playerClaimingId = 0;
            _recoverTimer = 0;
            _zonesDelay = new List<ZonesDelayRemove>();
            _consumeTokenTimer = 3600;
            _toSiegeTimer = 3600;
            _tokensToClaim = 1000;
            _tokensToSiege = 1000;
            _isSieging = false;
            _isSieged = false;
            _zoneDeactivationTimer = 86400;
            _gpsUpdateDelay = 30;
            _distanceToSiege = 3000;
            _triggerInit = false;
            _playerSiegingId = 0;
            _jdSiegingId = 0;
            _siegeTimer = 0;
            _discordChannelId = 0;
            _blockOwner = "";
            _siegeNoficationFreq = 10;
            _factionId = 0;
            _emissiveState = EmissiveState.Offline;
            _perks = new Dictionary<PerkType, PerkBase>();
            _uiControls = UIControls.Claiming;
            _uiPerkList = PerkTypeList.Production;
            _uiPlayerPerkList = PlayerPerks.Production;
            _isSiegingFinal = false;
            _isSiegedFinal = false;
            _version = SettingsVersion;
            _isCooling = false;
            _toSiegeFinalTimer = 3600;
            _tokensToSiegeFinal = 500;
            _tokensToDelaySiege = 500;
            _siegeDelayed = 0;
            _timeToDelay = 6;
            _siegeDelayAllowed = 3;
            _cooldownTime = 1800;
            _siegedBy = "";
            _readyToSiege = false;
            _timeframeToSiege = 1800;
            _centerToPlanet = false;
            _planetCenter = new Vector3D();
            _adminAllowSafeZoneAllies = false;
            _adminAllowTerritoryAllies = false;
            _allowSafeZoneAllies = false;
            _allowTerritoryAllies = false;
            _allowTools = true;
            _allowDrilling = false;
            _allowWelding = false;
            _allowGrinding = false;
            _planetName = "";
            _consumptionItem = "MyObjectBuilder_Component/ZoneChip";
            _consumptionAmt = 1;
            _isSiegeCooling = false;
            _siegeCoolingTime = 604800;
            _territoryStatus = TerritoryStatus.Neutral;
            _server = new ServerData();
            //_jdSieging = null;
            //_jdClaiming = null;
            //_playerClaiming = null;
            //_block = null;
            //_gpsData = new List<GpsData>();
            //_gridsInside = new Dictionary<long, GridData>();
            _accessTypeFactions = MySafeZoneAccess.Whitelist;
            _accessTypeFloatingObjects = MySafeZoneAccess.Blacklist;
            _accessTypeGrids = MySafeZoneAccess.Blacklist;
            _accessTypePlayers = MySafeZoneAccess.Whitelist;
            _factions = new List<long>();
            _visibleSZ = true;
			_factionsRadar = new List<long>();
            _factionsExempt = new List<long>();
			_szTexture = 0;
            _szColor = Color.Gray;
            _neutralEnemies = false;
            _finalSiegeDateTime = null;
            _discordGlobalChannelId = 0;
            _allianceId = 0;
            _previousOwnerId = 0;
		}

        public ClaimBlockSettings(long blockId, Vector3D pos, IMyTerminalBlock block)
        {
            _entityId = blockId;
            _safeZoneSize = 5000f;
            _claimRadius = 500000; //IsOnPlanet(pos);
            //Sync = false;
            _blockPos = pos;
            _claimedFaction = "";
            _safeZoneEntity = 0;
            _enabled = false;
            _isClaimed = false;
            _toClaimTimer = 900;
            //_playersInside = new Dictionary<long, PlayerData>();
            _unclaimName = "";
            _claimZoneName = _unclaimName;
            _safeZones = new List<long>();
            _timer = 0;
            _isClaiming = false;
            _distanceToClaim = 5000;
            _detailInfo = "";
            _jdClaimingId = 0;
            _playerClaimingId = 0;
            _recoverTimer = 0;
            _zonesDelay = new List<ZonesDelayRemove>();
            _consumeTokenTimer = 3600;
            _toSiegeTimer = 900;
            _tokensToClaim = 1000;
            _tokensToSiege = 500;
            _isSieging = false;
            _isSieged = false;
            _zoneDeactivationTimer = 86400;
            _gpsUpdateDelay = 30;
            _distanceToSiege = 15000;
            _triggerInit = false;
            _playerSiegingId = 0;
            _jdSiegingId = 0;
            _siegeTimer = 0;
            _discordChannelId = 0;
            _blockOwner = "";
            _siegeNoficationFreq = 30;
            _factionId = 0;
            _emissiveState = EmissiveState.Offline;
            _perks = new Dictionary<PerkType, PerkBase>();
            _uiControls = UIControls.Claiming;
            _uiPerkList = PerkTypeList.Production;
            _uiPlayerPerkList = PlayerPerks.Production;
            _isSiegingFinal = false;
            _isSiegedFinal = false;
            _version = SettingsVersion;
            _isCooling = false;
            _toSiegeFinalTimer = 900;
            _tokensToSiegeFinal = 500;
            _tokensToDelaySiege = 1000;
            _siegeDelayed = 0;
            _timeToDelay = 1;
            _siegeDelayAllowed = 1;
            _cooldownTime = 900;
            _siegedBy = "";
            _readyToSiege = false;
            _timeframeToSiege = 2700;
            _centerToPlanet = false;
            _planetCenter = new Vector3D();
            _adminAllowSafeZoneAllies = false;
            _adminAllowTerritoryAllies = false;
            _allowSafeZoneAllies = false;
            _allowTerritoryAllies = false;
            _allowTools = true;
            _allowDrilling = false;
            _allowWelding = false;
            _allowGrinding = false;
            _planetName = "";
            _consumptionItem = "MyObjectBuilder_Component/GoldPressedLatinum";
            _consumptionAmt = 3;
            _isSiegeCooling = false;
            _siegeCoolingTime = 259200;
            _territoryStatus = TerritoryStatus.Neutral;
            _server = new ServerData(block);
            //_jdSieging = null;
            //_jdClaiming = null;
            //_playerClaiming = null;
            //_block = block;
            //_gpsData = new List<GpsData>();
            //_gridsInside = new Dictionary<long, GridData>();
            _accessTypeFactions = MySafeZoneAccess.Whitelist;
            _accessTypeFloatingObjects = MySafeZoneAccess.Blacklist;
            _accessTypeGrids = MySafeZoneAccess.Blacklist;
            _accessTypePlayers = MySafeZoneAccess.Whitelist;
            _factions = new List<long>();
            _visibleSZ = true;
			_factionsRadar = new List<long>();
            _factionsExempt = new List<long>();
			_szTexture = 0;
			_szColor = Color.Gray;
            _neutralEnemies = false;
            _finalSiegeDateTime = null;
            _discordGlobalChannelId = 0;
			_allianceId = 0;
			_previousOwnerId = 0;
		}


        public ulong AllianceChannelId
        {
            get
            {
                if (Session.Instance.AllianceChannelIds.ContainsKey(_allianceId))
                    return Session.Instance.AllianceChannelIds[_allianceId];
                else
                    return 0;
            }
        }


		public MySafeZoneAccess AccessTypeFactions
        {
            get
            {
                return _accessTypeFactions;
            }

            set
            {
                if (_accessTypeFactions != value)
                {
                    _accessTypeFactions = value;
                    IMyEntity zone;
                    if (MyAPIGateway.Entities.TryGetEntityById(_safeZoneEntity, out zone) && zone is MySafeZone)
                    {
                        (zone as MySafeZone).AccessTypeFactions = value;
					}
					_server.Sync = true;
				}
            }
        }

        public MySafeZoneAccess AccessTypeFloatingObjects
        {
            get
            {
                return _accessTypeFloatingObjects;
            }

            set
            {
                if (_accessTypeFloatingObjects != value)
                {
                    _accessTypeFloatingObjects = value;
					_server.Sync = true;
				}
            }
        }

        public MySafeZoneAccess AccessTypeGrids
        {
            get
            {
                return _accessTypeGrids;
            }

            set
            {
                if (_accessTypeGrids != value)
                {
                    _accessTypeGrids = value;
					_server.Sync = true;
				}
            }
        }

        public MySafeZoneAccess AccessTypePlayers
        {
            get
            {
                return _accessTypePlayers;
            }

            set
            {
                if (_accessTypePlayers != value)
                {
                    _accessTypePlayers = value;
					_server.Sync = true;
				}
            }
        }

        public List<long> Factions
        {
            get 
            { 
                return _factions; 
            }

            set
            {
                if (_factionId != 0 && !value.Contains(_factionId))
                {
                    List<long> newList = new List<long> { _factionId };
                    newList.AddRange(value);
                    _factions = newList;
                }
                else
                    _factions = value;
                //_server.Sync = true;

                MySafeZone zone = null;
                var zones = MySessionComponentSafeZones.SafeZones;
                foreach (var sz in zones)
                {
                    if (sz == null || sz.MarkedForClose) continue;
                    if (sz.PositionComp.WorldVolume.Contains(BlockPos) == ContainmentType.Contains)
                    {
                        zone = sz;
                        break;
                    }
                }
                if (zone == null) return;
                List<IMyFaction> list = new List<IMyFaction>();
                foreach (long fid in Factions)
                {
                    IMyFaction f = MyAPIGateway.Session.Factions.TryGetFactionById(fid);
                    if (f != null)
                        list.Add(f);
                }
                zone.Factions = Comms.ListKonverter(zone.Factions[0], list);
				zone.Radius = SafeZoneSize;
				zone.RecreatePhysics();
				Comms.SyncSettingType(this, null, SyncType.FactionWhitelist);
            }
        }


		public List<long> FactionsRadar
		{
			get
			{
				return _factionsRadar;
			}

            set
            {
                if (_factionId != 0 && !value.Contains(_factionId))
                {
                    value.Insert(0, _factionId);
                }
                _factionsRadar = value;
                _server.Sync = true;
            }
		}

		public List<long> FactionsExempt
		{
			get
			{
				return _factionsExempt;
			}

            set
            {
                _factionsExempt = value;

                GPS.RemoveCachedGps(0, GpsType.Tag, this);
                GPS.RemoveCachedGps(0, GpsType.Player, this);

                _server.Sync = true;
            }
		}

		public bool NeutralEnemies
        {
            get
            {
                return _neutralEnemies;
            }

            set
            {
                _neutralEnemies = value;

                List<long> list = new List<long>() { FactionId };
                var factions = MyAPIGateway.Session.Factions.Factions;
                foreach (var faction in factions.Values)
                {
                    if (faction.IsEveryoneNpc()) continue;
                    if (faction.FactionId == FactionId) continue;
                    var relation = MyAPIGateway.Session.Factions.GetRelationBetweenFactions(faction.FactionId, FactionId);
                    if (relation == MyRelationsBetweenFactions.Enemies) continue;
                    if (value && relation == MyRelationsBetweenFactions.Neutral) continue;

                    list.Add(faction.FactionId);
                }
                FactionsExempt = list;
            }
        }

		public bool VisibleSZ
        {
            get
            {
                return _visibleSZ;
            }

            set
            {
                _visibleSZ = value;
                IMyEntity entity;
                if (MyAPIGateway.Entities.TryGetEntityById(_safeZoneEntity, out entity) && entity is MySafeZone)
                {
                    MySafeZone zone = entity as MySafeZone;
                    MyObjectBuilder_SafeZone ob = zone.GetObjectBuilder(true) as MyObjectBuilder_SafeZone;
                    if (!value)
                    {
                        ob.Texture = "SafeZone_Texture_Disabled";
                        ob.ModelColor = Color.Black.ToVector3();
                    }
                    else
                    {
                        ob.Texture = ActionControls.SZTextures[SZTexture].Value.String;
                        ob.ModelColor = SZColor.ToVector3();
                    }
                    MySessionComponentSafeZones.UpdateSafeZone(ob);
                    zone.Radius = SafeZoneSize;
                    zone.RecreatePhysics();
                }
                _server.Sync = true;
            }
        }

		public long SZTexture
		{
			get
			{
				return _szTexture;
			}

            set
            {
                _szTexture = value;
                IMyEntity entity;
                if (MyAPIGateway.Entities.TryGetEntityById(_safeZoneEntity, out entity) && entity is MySafeZone)
                {
                    MySafeZone zone = entity as MySafeZone;
                    MyObjectBuilder_SafeZone ob = zone.GetObjectBuilder(true) as MyObjectBuilder_SafeZone;
                    ob.Texture = ActionControls.SZTextures[SZTexture].Value.String ?? "SafeZone_Texture_Disabled";
                    MySessionComponentSafeZones.UpdateSafeZone(ob);
                    zone.Radius = SafeZoneSize;
                    zone.RecreatePhysics();
                }
                _server.Sync = true;
            }
		}

		public Color SZColor
        {
            get
            {
                return _szColor;
            }

            set
            {
                _szColor = value;
                IMyEntity entity;
                if (MyAPIGateway.Entities.TryGetEntityById(_safeZoneEntity, out entity) && entity is MySafeZone)
                {
                    MySafeZone zone = entity as MySafeZone;
                    MyObjectBuilder_SafeZone ob = zone.GetObjectBuilder(true) as MyObjectBuilder_SafeZone;
                    ob.ModelColor = value.ToVector3();
                    MySessionComponentSafeZones.UpdateSafeZone(ob);
					zone.Radius = SafeZoneSize;
					zone.RecreatePhysics();
				}
                _server.Sync = true;
            }
        }

		public TerritoryStatus GetTerritoryStatus
        {
            get { return _territoryStatus; }
            set
            {
                _territoryStatus = value;
                _server.Sync = true;
            }
        }

        public bool IsSiegeCooling
        {
            get { return _isSiegeCooling; }
            set
            {
                _isSiegeCooling = value;
                _server.Sync = true;
            }
        }

        public int SiegeCoolingTime
        {
            get { return _siegeCoolingTime; }
            set
            {
                _siegeCoolingTime = value;
                _server.Sync = true;
            }
        }

        public int ConsumptinAmt
        {
            get { return _consumptionAmt; }
            set
            {
                _consumptionAmt = value;
                _server.Sync = true;
            }
        }

        public string ConsumptionItem
        {
            get { return _consumptionItem; }
            set
            {
                _consumptionItem = value;
                _server.Sync = true;
            }
        }

        public string PlanetName
        {
            get { return _planetName; }
            set
            {
                _planetName = value;
                _server.Sync = true;
            }
        }

        public bool AllowGrinding
        {
            get { return _allowGrinding; }
            set
            {
                _allowGrinding = value;
                _server.Sync = true;
            }
        }

        public bool AllowWelding
        {
            get { return _allowWelding; }
            set
            {
                _allowWelding = value;
                _server.Sync = true;
            }
        }

        public bool AllowDrilling
        {
            get { return _allowDrilling; }
            set
            {
                _allowDrilling = value;
                _server.Sync = true;
            }
        }

        public bool AllowTools
        {
            get { return _allowTools; }
            set
            {
                _allowTools = value;
                _server.Sync = true;
            }
        }
        
        public bool AllowTerritoryAllies
        {
            get { return _allowTerritoryAllies; }
            set
            {
                _allowTerritoryAllies = value;
                _server.Sync = true;
            }
        }

        public bool AllowSafeZoneAllies
        {
            get { return _allowSafeZoneAllies; }
            set
            {
                _allowSafeZoneAllies = value;
                _server.Sync = true;

                if (!IsClaimed) return;
                if (Session.Instance.isServer)
                {
                    Utils.RemoveSafeZone(this);
                    if (IsSieging || IsSiegingFinal)
                        Utils.AddSafeZone(this, false);
                    else
                        Utils.AddSafeZone(this);
                }
                else
                {
                    Comms.UpdateSafeZoneAllies(this);
                }
            }
        }
            

        public bool AdminAllowTerritoryAllies
        {
            get { return _adminAllowTerritoryAllies; }
            set
            {
                _adminAllowTerritoryAllies = value;
                _server.Sync = true;
            }
        }

        public bool AdminAllowSafeZoneAllies
        {
            get { return _adminAllowSafeZoneAllies; }
            set
            {
                _adminAllowSafeZoneAllies = value;
                _server.Sync = true;
            }
        }

        public Vector3D PlanetCenter
        {
            get { return _planetCenter; }
            set
            {
                _planetCenter = value;
                _server.Sync = true;
            }
        }

        public bool CenterToPlanet
        {
            get { return _centerToPlanet; }
            set
            {
                _centerToPlanet = value;
                _server.Sync = true;
            }
        }

        public bool ReadyToSiege
        {
            get { return _readyToSiege; }
            set
            {
                _readyToSiege = value;
                _server.Sync = true;
            }
        }

        public int TimeframeToSiege
        {
            get { return _timeframeToSiege; }
            set
            {
                _timeframeToSiege = value;
                _server.Sync = true;
            }
        }

        public bool IsSiegingFinal
        {
            get { return _isSiegingFinal; }
            set
            {
                _isSiegingFinal = value;
                _server.Sync = true;
            }
        }

        public bool IsSiegeFinal
        {
            get { return _isSiegedFinal; }
            set
            {
                _isSiegedFinal = value;
                _server.Sync = true;
            }
        }

        public bool IsCooling
        {
            get { return _isCooling; }  
            set
            {
                _isCooling = value;
                _server.Sync = true;
            }
        }

        public int SiegeFinalTimer
        {
            get { return _toSiegeFinalTimer; }
            set
            {
                _toSiegeFinalTimer = value;
                _server.Sync = true;
            }
        }

        public int TokensSiegeFinal
        {
            get { return _tokensToSiegeFinal; } 
            set
            {
                _tokensToSiegeFinal = value;
                _server.Sync = true;
            }
        }

        public int SiegeDelayAllow
        {
            get { return _siegeDelayAllowed; }
            set
            {
                _siegeDelayAllowed = value;
                _server.Sync = true;
            }
        }

        public int TokensSiegeDelay
        {
            get { return _tokensToDelaySiege; } 
            set
            {
                _tokensToDelaySiege = value;
                _server.Sync = true;
            }
        }

        public int SiegedDelayedHit
        {
            get { return _siegeDelayed; }
            set
            {
                _siegeDelayed = value;
                _server.Sync = true;
            }
        }

        public int HoursToDelay
        {
            get { return _timeToDelay; }
            set
            {
                _timeToDelay = value;
                _server.Sync = true;
            }
        }

        public PlayerPerks UIPlayerPerks
        {
            get { return _uiPlayerPerkList; }
            set
            {
                _uiPlayerPerkList = value;
                _server.Sync = true;
            }
        }

        public int CooldownTimer
        {
            get { return _cooldownTime; }
            set
            {
                _cooldownTime = value;
                _server.Sync = true;
            }
        }

        public string SiegedBy
        {
            get { return _siegedBy; }
            set
            {
                _siegedBy = value;
                _server.Sync = true;
            }
        }

        public PerkTypeList UIPerkList
        {
            get { return _uiPerkList; }
            set
            {
                _uiPerkList = value;
                _server.Sync = true;
            }
        }

        public UIControls UI
        {
            get { return _uiControls; }
            set
            {
                _uiControls = value;
                _server.Sync = true;
            }
        }

        public Dictionary<PerkType, PerkBase> GetPerks
        {
            get { return _perks; }
        }

        public void UpdatePerks(PerkType perkType, bool add)
        {
            if (_perks == null)
                _perks = new Dictionary<PerkType, PerkBase>();

            if (add)
            {
                if (_perks.ContainsKey(perkType)) return;
                _perks.Add(perkType, new PerkBase(perkType, true));
                Comms.SyncSettingType(this, MyAPIGateway.Session.LocalHumanPlayer, SyncType.AddProductionPerk);

                /*if (IsClaimed)
                {
                    if (perkType == PerkType.Production)
                        Comms.SendApplyProductionPerkToServer(this);
                }*/
                    
            }
            else
            {
                if (!_perks.ContainsKey(perkType)) return;
                if (IsClaimed)
                {
                    if (perkType == PerkType.Production)
                    {
                        Utils.RemovePerkType(this, PerkType.Production);
                        //Comms.SendRemoveProductionPerkToServer(this);
                    };
                }
            }
        }

        public ServerData Server
        {
            get { return _server; }
            set { _server = value; }
        }

        public EmissiveState BlockEmissive
        {
            get { return _emissiveState; }
            set
            {
                _emissiveState = value;
                //_server.Sync = true;
                IMyPlayer player = MyAPIGateway.Session.LocalHumanPlayer;
                //Comms.SyncSettingsToOthers(this, player);

                if ((Session.Instance.isServer && !Session.Instance.isDedicated) || !Session.Instance.isServer)
                    Utils.SetEmissive(value, Block as IMyBeacon);

                Comms.SyncSettingType(this, player, SyncType.Emissive);
                //Comms.UpdateEmissiveToClients(_entityId, MyAPIGateway.Session.LocalHumanPlayer);
            }
        }

        public long FactionId
        {
            get { return _factionId; }
            set
            {
                _factionId = value;
                _server.Sync = true;
            }
        }

        public int SiegeNotificationFreq
        {
            get { return _siegeNoficationFreq; }
            set
            {
                _siegeNoficationFreq = value;
                _server.Sync = true;
            }
        }

        public string BlockOwner
        {
            get { return _blockOwner; }
            set
            {
                _blockOwner = value;
                _server.Sync = true;
            }
        }

        public ulong DiscordChannelId
        {
            get { return _discordChannelId; }
            set
            {
                _discordChannelId = value;
                _server.Sync = true;
            }
        }

		public ulong DiscordGlobalChannelId
		{
			get { return _discordGlobalChannelId; }
			set
			{
				_discordGlobalChannelId = value;
				_server.Sync = true;
			}
		}

		public int SiegeTimer
        {
            get { return _siegeTimer; }
            set
            {
                _siegeTimer = value;
				//_server.Sync = true;
				if (Session.Instance.isServer || Session.Instance.isDedicated)
					Comms.SyncSettingType(this, null, SyncType.SiegeTimer);
			}
        }

        public long PlayerSiegingId
        {
            get { return _playerSiegingId; }    
            set
            {
                _playerSiegingId = value;
                _server.Sync = true;
            }
        }

        public long JDSiegingId
        {
            get { return _jdSiegingId; }
            set
            {
                _jdSiegingId = value;
                _server.Sync = true;
            }
        }

        public bool TriggerInit
        {
            get { return _triggerInit; }
            set
            {
                _triggerInit = value;
                _server.Sync = true;
            }
        }

        public double DistanceToSiege
        {
            get { return _distanceToSiege; }
            set
            {
                _distanceToSiege = value;
                _server.Sync = true;
            }
        }

        public int GpsUpdateDelay
        {
            get { return _gpsUpdateDelay; }
            set
            {
                _gpsUpdateDelay = value;
                _server.Sync = true;
            }
        }

        public int ToClaimTimer
        {
            get { return _toClaimTimer; }
            set
            {
                _toClaimTimer = value;
                _server.Sync = true;
            }
        }

        public int ConsumeTokenTimer
        {
            get { return _consumeTokenTimer; }
            set
            {
                _consumeTokenTimer = value;
                _server.Sync = true;
            }
        }

        public int ToSiegeTimer
        {
            get { return _toSiegeTimer; }
            set
            {
                _toSiegeTimer = value;
                _server.Sync = true;
            }
        }

        public int TokensToClaim
        {
            get { return _tokensToClaim; }
            set
            {
                _tokensToClaim = value;
                _server.Sync = true;
            }
        }

        public int TokensToSiege
        {
            get { return _tokensToSiege; }
            set
            {
                _tokensToSiege = value;
                _server.Sync = true;
            }
        }

        public bool IsSieging
        {
            get { return _isSieging; }
            set
            {
                _isSieging = value;
                _server.Sync = true;
            }
        }

        public bool IsSieged
        {
            get { return _isSieged; }
            set
            {
                _isSieged = value;
                _server.Sync = true;
            }
        }

        public int ZoneDeactivationTimer
        {
            get { return _zoneDeactivationTimer; }
            set
            {
                _zoneDeactivationTimer = value;
                _server.Sync = true;
            }
        }

        public long EntityId
        {
            get { return _entityId; }
            set
            {
                _entityId = value;
                _server.Sync = true;
            }
        }

        public float SafeZoneSize
        {
            get { return _safeZoneSize; }
            set
            {
                _safeZoneSize = value;
                _server.Sync = true;
                //Session.Instance.claimBlocks[_entityId]._safeZoneSize = value;
                //Session.Instance.claimBlocks[_entityId]._server.Sync = true;
            }
        }

        public Vector3D BlockPos
        {
            get { return _blockPos; }
            set
            {
                _blockPos = value;
                _server.Sync = true;
                //Session.Instance.claimBlocks[_entityId]._blockPos = value;
                //Session.Instance.claimBlocks[_entityId]._server.Sync = true;
            }
        }

        public string ClaimedFaction
        {
            get { return _claimedFaction; }
            set
            {
                _claimedFaction = value;
                _server.Sync = true;
                //Session.Instance.claimBlocks[_entityId]._claimedFaction = value;
                //Session.Instance.claimBlocks[_entityId]._server.Sync = true;
            }
        }


        public float ClaimRadius
        {
            get { return _claimRadius; }
            set
            {
                _claimRadius = value;
                _server.Sync = true;
                //Session.Instance.claimBlocks[_entityId]._claimRadius = value;
                //Session.Instance.claimBlocks[_entityId]._server.Sync = true;
            }
        }

        public long SafeZoneEntity
        {
            get { return _safeZoneEntity; }
            set
            {
                _safeZoneEntity = value;
                _server.Sync = true;
            }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                IMyPlayer player = MyAPIGateway.Session.LocalHumanPlayer;
                Comms.SyncSettingsToOthers(this, player);
                //_server.Sync = true;
            }
        }

        public bool IsClaimed
        {
            get { return _isClaimed; }
            set
            {
                _isClaimed = value;
                _server.Sync = true;
                //Session.Instance.claimBlocks[_entityId]._isClaimed = value;
                //Session.Instance.claimBlocks[_entityId]._server.Sync = true;
            }
        }

        public Dictionary<long, PlayerData> GetPlayersInside
        {
            get 
            {
                if (_server._playersInside.Count == 0)
                    NexusGPSMessage.SendRemoveGPS(this, null);
                return _server._playersInside; 
            }
        }

        public void UpdatePlayerInside(long player, bool add)
        {
            if (add)
            {
                if (_server._playersInside.ContainsKey(player)) return;
                _server._playersInside.Add(player, new PlayerData(player));
            }

            else
            {
                if (player == 0)
                {
                    GPS.RemoveCachedGps(0, GpsType.Player, this);
                    _server._playersInside.Clear();
                    return;
                }
                    

                if (_server._playersInside.ContainsKey(player))
                {
                    GPS.RemoveCachedGps(0, GpsType.Player, this, 0, player);
                    _server._playersInside.Remove(player);
                }
            }
                

            _server.Sync = true;
            //Session.Instance.claimBlocks[_entityId]._playersInside = _playersInside;
            //Session.Instance.claimBlocks[_entityId]._server.Sync = true;
        }

        public string ClaimZoneName
        {
            get { return _claimZoneName; }
            set
            {
                _claimZoneName = value;
                _server.Sync = true;
                //Session.Instance.claimBlocks[_entityId]._claimZoneName = value;
                //Session.Instance.claimBlocks[_entityId]._server.Sync = true;
            }
        }

        public string TerritoryName
        {
            get { return _unclaimName; }
            set
            {
                _unclaimName = value;
                _server.Sync = true;
                //Session.Instance.claimBlocks[_entityId]._unclaimName = value;
                //Session.Instance.claimBlocks[_entityId]._server.Sync = true;
            }
        }

        public List<long> GetSafeZones
        {
            get { return _safeZones; }
        }

        public void UpdateSafeZones(long blockId, bool add)
        {
            if (add)
            {
                if (_safeZones.Contains(blockId)) return;
                _safeZones.Add(blockId);
            }
            else
                if (_safeZones.Contains(blockId)) _safeZones.Remove(blockId);

            _server.Sync = true;
            //Session.Instance.claimBlocks[_entityId]._safeZoneBlocks = _safeZoneBlocks;
            //Session.Instance.claimBlocks[_entityId]._server.Sync = true;
        }

        public int Timer
        {
            get { return _timer; }
            set
            {
                _timer = value;
				//_server.Sync = true;

				if (Session.Instance.isServer || Session.Instance.isDedicated)
					Comms.SyncSettingType(this, null, SyncType.Timer);
				//Session.Instance.claimBlocks[_entityId]._timer = value;
				//Session.Instance.claimBlocks[_entityId]._server.Sync = true;
			}
        }

        public bool IsClaiming
        {
            get { return _isClaiming; }
            set
            {
                _isClaiming = value;
                _server.Sync = true;
                //Session.Instance.claimBlocks[_entityId]._isClaiming = value;
                //Session.Instance.claimBlocks[_entityId]._server.Sync = true;
            }
        }

        public double DistanceToClaim
        {
            get { return _distanceToClaim; }
            set
            {
                _distanceToClaim = value;
                _server.Sync = true;
                //Session.Instance.claimBlocks[_entityId]._distanceToClaim = value;
                //Session.Instance.claimBlocks[_entityId]._server.Sync = true;
            }
        }

        public string DetailInfo
        {
            get { return _detailInfo; }
            set
            {
                _detailInfo = value;
                IMyPlayer player = MyAPIGateway.Session.LocalHumanPlayer;
                //Comms.SyncSettingsToOthers(this, player);
                

                //_server.Sync = true;
                if (Session.Instance.claimBlocks[_entityId].Block != null)
                {
                    IMyTerminalBlock block = Session.Instance.claimBlocks[_entityId].Block;
                    block.RefreshCustomInfo();
                    ActionControls.RefreshControls(block, false);
                }

                Comms.SyncSettingType(this, player, SyncType.DetailInfo);
                //Comms.UpdateDetailInfo(value, _entityId);
            }
        }

        public long JDClaimingId
        {
            get { return _jdClaimingId; }
            set
            {
                _jdClaimingId = value;
                _server.Sync = true;
                //Session.Instance.claimBlocks[_entityId]._jdClaimingId = value;
                //Session.Instance.claimBlocks[_entityId]._server.Sync = true;
            }
        }

        public long PlayerClaimingId
        {
            get { return _playerClaimingId; }
            set
            {
                _playerClaimingId = value;
                _server.Sync = true;
                //Session.Instance.claimBlocks[_entityId]._playerClaimingId = value;
                //Session.Instance.claimBlocks[_entityId]._server.Sync = true;
            }
        }

        public IMyEntity JDSieging
        {
            get { return _server._jdSieging; }
            set { _server._jdSieging = value; }
        }

        public IMyEntity JDClaiming
        {
            get { return _server._jdClaiming; }
            set { _server._jdClaiming = value; }
        }

        public IMyPlayer PlayerClaiming
        {
            get { return _server._playerClaiming; }
            set { _server._playerClaiming = value; }
        }

        public int RecoveryTimer
        {
            get { return _recoverTimer; }
            set
            {
                _recoverTimer = value;
                _server.Sync = true;
                //Session.Instance.claimBlocks[_entityId]._recoverTimer = value;
                //Session.Instance.claimBlocks[_entityId]._server.Sync = true;
            }
        }

        public DateTime? FinalSiegeDateTime
        {
            get { return _finalSiegeDateTime; }
            set 
            {
                _finalSiegeDateTime = value;
			    Comms.SyncSettingType(this, null, SyncType.FinalSiegeDateTime);
			}
        }

        public IMyTerminalBlock Block
        {
            get { return _server._block; }
            set { _server._block = value; }
        }

        public List<ZonesDelayRemove> GetZonesDelayRemove
        {
            get { return _zonesDelay; }
        }

        public void UpdateZonesDelayRemove(long id, DateTime time, bool add)
        {
            if (add)
            {
                if (_zonesDelay == null)
                    _zonesDelay = new List<ZonesDelayRemove>();

                _zonesDelay.Add(new ZonesDelayRemove(id, time));
            }
            else
            {
                _zonesDelay.RemoveAll(x => x.zoneId == id);
            }

            _server.Sync = true;
            //MyVisualScriptLogicProvider.ShowNotification($"SafeZoneDelay Count = {Session.Instance.claimBlocks[_entityId].GetZonesDelayRemove.Count}", 8000);
        }

        public Dictionary<long, GridData> GetGridsInside
        {
            get { return _server._gridsInside; }
        }

        public void UpdateGridsInside(long gridId, MyCubeGrid grid, bool add)
        {
            if (add && !_server._gridsInside.ContainsKey(gridId))
            {
                _server._gridsInside.Add(gridId, new GridData(gridId, grid));
            }
            else if (!add && _server._gridsInside.ContainsKey(gridId))
            {
                if (_server._gridsInside[gridId].gpsData.Count != 0)
                    GPS.RemoveCachedGps(0, GpsType.Tag, this, gridId);

                _server._gridsInside.Remove(gridId);
            }

			//if (grid == null) return;
			//Session.Instance.claimBlocks[_entityId]._gridsInside = _gridsInside;
			//var cubegrid = grid as IMyCubeGrid;
			//MyVisualScriptLogicProvider.ShowNotification($"Grids Inside = {Session.Instance.claimBlocks[_entityId]._server._gridsInside.Count}, {cubegrid.CustomName}", 10000);
		}

        public void UpdateGridData(long gridId, GridChangeType type, bool value)
        {
            if (!_server._gridsInside.ContainsKey(gridId)) return;

            if (type == GridChangeType.Controller)
                _server._gridsInside[gridId].hasController = value;

            if (type == GridChangeType.Power)
                _server._gridsInside[gridId].hasPower = value;

            //Session.Instance.claimBlocks[_entityId]._gridsInside = _gridsInside;
        }

        public List<GpsData> GetGpsData(long gridId)
        {
            if (_server._gridsInside.ContainsKey(gridId))
                return _server._gridsInside[gridId].gpsData;

            return new List<GpsData>();
        }

        public void UpdateGpsData(GpsData data, bool add)
        {
            if(data.gpsType == GpsType.Tag || data.gpsType == GpsType.Block)
            {
                if (_server._gridsInside == null)
                    _server._gridsInside = new Dictionary<long, GridData>();

                if (!_server._gridsInside.ContainsKey(data.entity.EntityId))
                    UpdateGridsInside(data.entity.EntityId, data.entity as MyCubeGrid, true);

                if (add)
                {
                    if (!_server._gridsInside.ContainsKey(data.entity.EntityId)) return;
                    _server._gridsInside[data.entity.EntityId].gpsData.Add(data);
                }
                else
                {
                    if (!_server._gridsInside.ContainsKey(data.entity.EntityId)) return;
                    _server._gridsInside[data.entity.EntityId].gpsData.Remove(data);
                }
            }

            if(data.gpsType == GpsType.Player)
            {
                if (_server._playersInside == null)
                    _server._playersInside = new Dictionary<long, PlayerData>();

                if (!_server._playersInside.ContainsKey(data.player.IdentityId))
                    UpdatePlayerInside(data.player.IdentityId, true);

                if (add)
                {
                    if (!_server._playersInside.ContainsKey(data.player.IdentityId)) return;
                    _server._playersInside[data.player.IdentityId].gpsData.Add(data);
                }
                else
                {
                    if (!_server._playersInside.ContainsKey(data.player.IdentityId)) return;
                    _server._playersInside[data.player.IdentityId].gpsData.Remove(data);
                }
            }

            //MyVisualScriptLogicProvider.ShowNotification($"Added gps data = {_gridsInside[data.entity.EntityId].gpsData.Count}", 8000);
            //Session.Instance.claimBlocks[_entityId]._gridsInside[data.entity.EntityId].gpsData = _gridsInside[data.entity.EntityId].gpsData;
        }

        public void UpdatesBlocksMonitored(MyCubeGrid grid, MyCubeBlock block, bool add, bool clearAll = false)
        {
            return;
            try
            {
                if (!_server._gridsInside.ContainsKey(grid.EntityId)) return;

                if (_server._gridsInside[grid.EntityId].blocksMonitored == null)
                    _server._gridsInside[grid.EntityId].blocksMonitored = new BlocksMonitored();

                if (clearAll)
                {
                    foreach (var blocks in _server._gridsInside[grid.EntityId].blocksMonitored.controllers)
                    {
                        blocks.IsWorkingChanged -= Events.IsWorkingChanged;
                    }

                    foreach (var blocks in _server._gridsInside[grid.EntityId].blocksMonitored.powers)
                    {
                        blocks.IsWorkingChanged -= Events.IsWorkingChanged;
                    }

                    foreach (var blocks in _server._gridsInside[grid.EntityId].blocksMonitored.tools)
                    {
                        blocks.IsWorkingChanged -= Events.IsWorkingChanged;
                    }

                    foreach (var blocks in _server._gridsInside[grid.EntityId].blocksMonitored.drills)
                    {
                        blocks.IsWorkingChanged -= Events.IsWorkingChanged;
                    }

                    foreach (var blocks in _server._gridsInside[grid.EntityId].blocksMonitored.production)
                    {
                        blocks.IsWorkingChanged -= Events.IsWorkingChanged;
                        blocks.OwnershipChanged -= Events.ProductionOwnershipChanged;
                    }

                    Utils.RemovePerksFromGrid(this, grid);

                    _server._gridsInside[grid.EntityId].blocksMonitored.controllers.Clear();
                    _server._gridsInside[grid.EntityId].blocksMonitored.powers.Clear();
                    _server._gridsInside[grid.EntityId].blocksMonitored.tools.Clear();
                    _server._gridsInside[grid.EntityId].blocksMonitored.drills.Clear();
                    _server._gridsInside[grid.EntityId].blocksMonitored.production.Clear();

                    //Session.Instance.claimBlocks[_entityId]._gridsInside[grid.EntityId].blocksMonitored = _gridsInside[grid.EntityId].blocksMonitored;
                    return;
                }

                if (add)
                {
                    if (block as IMyShipController != null)
                    {
                        if (_server._gridsInside[grid.EntityId].blocksMonitored.controllers.Contains(block as IMyShipController)) return;
                        _server._gridsInside[grid.EntityId].blocksMonitored.controllers.Add(block as IMyShipController);
                    }

                    if (block as IMyPowerProducer != null)
                    {
                        if (_server._gridsInside[grid.EntityId].blocksMonitored.powers.Contains(block as IMyPowerProducer)) return;
                        _server._gridsInside[grid.EntityId].blocksMonitored.powers.Add(block as IMyPowerProducer);
                        //MyVisualScriptLogicProvider.ShowNotification($"Block Count = {_gridsInside[grid.EntityId].blocksMonitored.powers.Count}", 5000);
                    }

                    if (block as IMyShipToolBase != null)
                    {
                        if (_server._gridsInside[grid.EntityId].blocksMonitored.tools.Contains(block as IMyShipToolBase)) return;
                        _server._gridsInside[grid.EntityId].blocksMonitored.tools.Add(block as IMyShipToolBase);
                    }

                    if (block as IMyShipDrill != null)
                    {
                        if (_server._gridsInside[grid.EntityId].blocksMonitored.drills.Contains(block as IMyShipDrill)) return;
                        _server._gridsInside[grid.EntityId].blocksMonitored.drills.Add(block as IMyShipDrill);
                    }

                    if (block as IMyProductionBlock != null)
                    {
                        if (_server._gridsInside[grid.EntityId].blocksMonitored.production.Contains(block as IMyProductionBlock)) return;
                        _server._gridsInside[grid.EntityId].blocksMonitored.production.Add(block as IMyProductionBlock);

                        var production = block as IMyProductionBlock;
                        production.OwnershipChanged += Events.ProductionOwnershipChanged;

                        if (IsClaimed)
                            Utils.AddAllProductionMultipliers(this, block, true);

                        //MyVisualScriptLogicProvider.ShowNotificationToAll($"Production block added event. Is Claimed = {IsClaimed}", 8000, "Red");
                    }

                    block.IsWorkingChanged += Events.IsWorkingChanged;
                }
                else
                {
                    if (block as IMyShipController != null)
                    {
                        if (!_server._gridsInside[grid.EntityId].blocksMonitored.controllers.Contains(block as IMyShipController)) return;
                        _server._gridsInside[grid.EntityId].blocksMonitored.controllers.Remove(block as IMyShipController);
                    }

                    if (block as IMyPowerProducer != null)
                    {
                        if (!_server._gridsInside[grid.EntityId].blocksMonitored.powers.Contains(block as IMyPowerProducer)) return;
                        _server._gridsInside[grid.EntityId].blocksMonitored.powers.Remove(block as IMyPowerProducer);
                    }

                    if (block as IMyShipToolBase != null)
                    {
                        if (!_server._gridsInside[grid.EntityId].blocksMonitored.tools.Contains(block as IMyShipToolBase)) return;
                        _server._gridsInside[grid.EntityId].blocksMonitored.tools.Remove(block as IMyShipToolBase);
                    }

                    if (block as IMyShipDrill != null)
                    {
                        if (!_server._gridsInside[grid.EntityId].blocksMonitored.drills.Contains(block as IMyShipDrill)) return;
                        _server._gridsInside[grid.EntityId].blocksMonitored.drills.Remove(block as IMyShipDrill);
                    }

                    if (block as IMyProductionBlock != null)
                    {
                        if (!_server._gridsInside[grid.EntityId].blocksMonitored.production.Contains(block as IMyProductionBlock)) return;

                        if (GetPerks.ContainsKey(PerkType.Production))
                        {
                            Utils.RemoveProductionMultipliers(this, block as IMyProductionBlock, true);
                        }

                        _server._gridsInside[grid.EntityId].blocksMonitored.production.Remove(block as IMyProductionBlock);

                        var production = block as IMyProductionBlock;
                        production.OwnershipChanged -= Events.ProductionOwnershipChanged;
                    }

                    //_gridsInside[grid.EntityId].blocksMonitored.Remove(block);
                    block.IsWorkingChanged -= Events.IsWorkingChanged;
                }
                
            }
            catch(Exception ex)
            {

            }

            //Session.Instance.claimBlocks[_entityId]._gridsInside[grid.EntityId].blocksMonitored = _gridsInside[grid.EntityId].blocksMonitored;
        }

        private float IsOnPlanet(Vector3D pos)
        {
            if (MyVisualScriptLogicProvider.IsPlanetNearby(pos))
                return 25000f;
            else
                return 50000f;
        }

        public enum GridChangeType
        {
            Power,
            Controller,
            Both
        }
    }

    public class ServerData
    {
        public IMyEntity _jdSieging;
        public IMyEntity _jdClaiming;
        public IMyPlayer _playerClaiming;
        public IMyTerminalBlock _block;
        public bool Sync;
        public Dictionary<long, GridData> _gridsInside;
        public Dictionary<long, PlayerData> _playersInside;
        public bool _perkWarning;
        public List<IMyProgrammableBlock> _pbList;

        public ServerData()
        {
            _jdSieging = null;
            _jdClaiming = null;
            _playerClaiming = null;
            _block = null;
            Sync = false;
            _gridsInside = new Dictionary<long, GridData>();
            _playersInside = new Dictionary<long, PlayerData>();
            _perkWarning = false;
            _pbList = new List<IMyProgrammableBlock>();
        }

        public ServerData(IMyTerminalBlock block)
        {
            _jdSieging = null;
            _jdClaiming = null;
            _playerClaiming = null;
            _block = block;
            Sync = false;
            _gridsInside = new Dictionary<long, GridData>();
            _playersInside = new Dictionary<long, PlayerData>();
            _perkWarning = false;
            _pbList = new List<IMyProgrammableBlock>();
        }
    }

    public enum PerkType
    {
        Production
    }

    [ProtoContract]
    public class PerkBase
    {
        [ProtoMember(50)] public bool enabled;
        [ProtoMember(51)] public PerkType type;
        [ProtoMember(52)] public Perk perk;
        [ProtoMember(53)] public PlayerPerks playerPerks;

        public PerkBase()
        {
            enabled = false;
            type = PerkType.Production;
            perk = new Perk();
            playerPerks = PlayerPerks.Production;
        }

        public PerkBase(PerkType perkType, bool enable)
        {
            enabled = enable;
            type = perkType;
            perk = new Perk(perkType);
            playerPerks = PlayerPerks.Production;

            //_server.Sync = true;
        }

        public PlayerPerks PlayerPerksUI
        {
            get { return playerPerks; }
            set { playerPerks = value; }
        }

        public bool Enable
        {
            get { return enabled; } 
            set
            {
                enabled = value;
                //perkSync = true;
            }
        }

        public int TotalPerkCost
        {
            get
            {
                int cost = 0;
                if (type == PerkType.Production)
                {
                    if (perk.productionPerk.allowClientControlSpeed && perk.productionPerk.enableClientControlSpeed)
                        cost += perk.productionPerk.speedTokens;

                    if (perk.productionPerk.allowClientControlYield && perk.productionPerk.enableClientControlYield)
                        cost += perk.productionPerk.yieldTokens;

                    if (perk.productionPerk.allowClientControlEnergy && perk.productionPerk.enableClientControlEnergy)
                        cost += perk.productionPerk.energyTokens;
                }


                return cost;
            }
        }

        public int ActivePerkCost
        {
            get
            {
                int cost = 0;
                if (type == PerkType.Production)
                {
                    if (perk.productionPerk.GetActiveUpgrades.Count == 0) return 0;
                    foreach(var upgrade in perk.productionPerk.GetActiveUpgrades)
                    {
                        if (upgrade == "Productivity")
                            cost += perk.productionPerk.speedTokens;

                        if (upgrade == "Effectiveness")
                            cost += perk.productionPerk.yieldTokens;

                        if (upgrade == "PowerEfficiency")
                            cost += perk.productionPerk.energyTokens;
                    }
                }

                return cost;
            }
        }

        public int PendingPerkCost
        {
            get
            {
                int cost = ActivePerkCost;
                if (type == PerkType.Production)
                {
                    if (perk.productionPerk.GetPendingAddUpgrades.Count != 0)
                    {
                        foreach (var upgrade in perk.productionPerk.GetPendingAddUpgrades)
                        {
                            if (upgrade == "Productivity")
                                cost += perk.productionPerk.speedTokens;

                            if (upgrade == "Effectiveness")
                                cost += perk.productionPerk.yieldTokens;

                            if (upgrade == "PowerEfficiency")
                                cost += perk.productionPerk.energyTokens;
                        }
                    }

                    if (perk.productionPerk.GetPendingRemoveUpgrades.Count != 0)
                    {
                        foreach (var upgrade in perk.productionPerk.GetPendingRemoveUpgrades)
                        {
                            if (upgrade == "Productivity")
                                cost -= perk.productionPerk.speedTokens;

                            if (upgrade == "Effectiveness")
                                cost -= perk.productionPerk.yieldTokens;

                            if (upgrade == "PowerEfficiency")
                                cost -= perk.productionPerk.energyTokens;
                        }
                    }
                }

                return cost;
            }
        }
    }

    [ProtoContract]
    public class Perk
    {
        [ProtoMember(53)] public ProductionPerk productionPerk;

        public Perk()
        {
            productionPerk = new ProductionPerk();
        }

        public Perk(PerkType perkType)
        {
            if(perkType == PerkType.Production)
            {
                productionPerk = new ProductionPerk();
            }
        }
    }

    [ProtoContract]
    public class ProductionPerk
    {
        [ProtoMember(54)] public float speed;
        [ProtoMember(55)] public float yield;
        [ProtoMember(56)] public float energy;
        [ProtoMember(57)] public List<long> attachedEntities;
        [ProtoMember(58)] public bool allowClientControlSpeed;
        [ProtoMember(59)] public bool allowClientControlYield;
        [ProtoMember(60)] public bool allowClientControlEnergy;
        [ProtoMember(61)] public bool allowStandAlone;
        [ProtoMember(62)] public int speedTokens;
        [ProtoMember(63)] public int yieldTokens;
        [ProtoMember(64)] public int energyTokens;
        [ProtoMember(65)] public bool enableClientControlSpeed;
        [ProtoMember(66)] public bool enableClientControlYield;
        [ProtoMember(67)] public bool enableClientControlEnergy;
        [ProtoMember(68)] public bool productionRunning;
        [ProtoMember(69)] public List<string> activeUpgrades;
        [ProtoMember(70)] public List<string> pendingAddUpgrades;
        [ProtoMember(71)] public List<string> pendingRemoveUpgrades;


        public ProductionPerk()
        {
            speed = 0f;
            yield = 0f;
            energy = 0f;
            attachedEntities = new List<long>();
            allowStandAlone = false;
            allowClientControlSpeed = false;
            allowClientControlYield = false;
            allowClientControlEnergy = false;
            speedTokens = 0;
            yieldTokens = 0;
            energyTokens = 0;
            enableClientControlSpeed = false;
            enableClientControlYield = false;
            enableClientControlEnergy = false;
            productionRunning = false;
            activeUpgrades = new List<string>();
            pendingAddUpgrades = new List<string>();
            pendingRemoveUpgrades = new List<string>();
        }

        public void PendingAddUpgrades(string upgradeName, bool add)
        {
            if (pendingAddUpgrades == null)
                pendingAddUpgrades = new List<string>();

            if (add)
            {
                if (pendingAddUpgrades.Contains(upgradeName)) return;
                pendingAddUpgrades.Add(upgradeName);
            }
            else
            {
                if (!pendingAddUpgrades.Contains(upgradeName)) return;
                pendingAddUpgrades.Remove(upgradeName);
            }
        }

        public void PendingRemoveUpgrades(string upgradeName, bool add)
        {
            if (pendingRemoveUpgrades == null)
                pendingRemoveUpgrades = new List<string>();

            if (add)
            {
                if (pendingRemoveUpgrades.Contains(upgradeName)) return;
                pendingRemoveUpgrades.Add(upgradeName);
            }
            else
            {
                if (!pendingRemoveUpgrades.Contains(upgradeName)) return;
                pendingRemoveUpgrades.Remove(upgradeName);
            }
        }

        public List<string> GetPendingAddUpgrades
        {
            get { return pendingAddUpgrades; }
        }

        public List<string> GetPendingRemoveUpgrades
        {
            get { return pendingRemoveUpgrades; }
        }

        public void ActiveUprades(string upgradeName, bool add)
        {
            if (activeUpgrades == null)
                activeUpgrades = new List<string>();

            if (add)
            {
                if (activeUpgrades.Contains(upgradeName)) return;
                activeUpgrades.Add(upgradeName);
            }
            else
            {
                if (!activeUpgrades.Contains(upgradeName)) return;
                activeUpgrades.Remove(upgradeName);
            }
        }

        public List<string> GetActiveUpgrades
        {
            get { return activeUpgrades; }
        }

        public bool ProductionRunning
        {
            get { return productionRunning; }
            set { productionRunning = value; }
        }

        public List<long> GetAttachedEntities
        {
            get { return attachedEntities; }
        }

        public void UpdateAttachedEntities(long entityId, bool add)
        {
            //MyVisualScriptLogicProvider.ShowNotificationToAll($"Added Attached Production EntityId = {entityId} | IsServer = {MyAPIGateway.Multiplayer.IsServer}", 20000, "Red");
            if (attachedEntities == null)
                attachedEntities = new List<long>();

            if (add)
            {
                if (attachedEntities.Contains(entityId)) return;
                attachedEntities.Add(entityId);
            }
            else
            {
                if (!attachedEntities.Contains(entityId)) return;
                attachedEntities.Remove(entityId);
            }

            //perkSync = true;
        }

        public float Speed
        {
            get { return speed; }   
            set
            {
                speed = value;
                //_server.Sync = true;
            }
        }

        public float Yield
        {
            get { return yield; }
            set
            {
                yield = value;
                //_server.Sync = true;
            }
        }

        public float Energy
        {
            get { return energy; }
            set
            {
                energy = value;
                //_server.Sync = true;
            }
        }
    }

    public class GridData
    {
        public long gridId;
        public MyCubeGrid cubeGrid;
        public bool hasController;
        public bool hasPower;
        public bool hasGps;
        public List<GpsData> gpsData = new List<GpsData>();
		public BlocksMonitored blocksMonitored;

        public GridData()
        {

        }

        public GridData(long Id, MyCubeGrid grid)
        {
            gridId = Id;
            cubeGrid = grid;
            hasController = false;
            hasPower = false;
            hasGps = false;
            gpsData = new List<GpsData>();
            blocksMonitored = new BlocksMonitored();

            //data.blocksMonitored.controllers = new List<IMyShipController>();
            //data.blocksMonitored.powers = new List<IMyPowerProducer>();
            //data.blocksMonitored.tools = new List<IMyShipToolBase>();
        }
    }

    public class PlayerData
    {
        public long playerId;
        public List<GpsData> gpsData = new List<GpsData>();

		public PlayerData()
        {

        }

        public PlayerData(long id)
        {
            playerId = id;
            gpsData = new List<GpsData>();
        }
    }

    public class BlocksMonitored
    {
        public List<IMyShipController> controllers = new List<IMyShipController>();
        public List<IMyPowerProducer> powers = new List<IMyPowerProducer>();
        public List<IMyShipToolBase> tools = new List<IMyShipToolBase>();
        public List<IMyShipDrill> drills = new List<IMyShipDrill>();
        public List<IMyProductionBlock> production = new List<IMyProductionBlock>();

        public BlocksMonitored()
        {
            controllers = new List<IMyShipController>();
            powers = new List<IMyPowerProducer>();
            tools = new List<IMyShipToolBase>();
            drills = new List<IMyShipDrill>();
            production = new List<IMyProductionBlock>();
        }
    }

    [ProtoContract]
    public class ZonesDelayRemove
    {
        [ProtoMember(100)]
        public long zoneId;

        [ProtoMember(101)]
        public DateTime time;

        public ZonesDelayRemove()
        {
            zoneId = 0;
            time = DateTime.Now;
        }

        public ZonesDelayRemove(long id, DateTime delay)
        {
            zoneId = id;
            time = delay;
        }
    }
}
