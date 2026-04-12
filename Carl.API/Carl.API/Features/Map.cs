namespace CarX.API.Features
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public static class Map
    {
        //private static readonly List<Room> RoomsValue = new List<Room>(250);
        //private static readonly List<Door> DoorsValue = new List<Door>(250);
        private static readonly List<Lift> LiftsValue = new List<Lift>(10);
        private static readonly List<TeslaGate> TeslasValue = new List<TeslaGate>(10);
        internal static readonly List<Ragdoll> RagdollsValue = new List<Ragdoll>();

        //private static readonly ReadOnlyCollection<Room> ReadOnlyRoomsValue = RoomsValue.AsReadOnly();
        //private static readonly ReadOnlyCollection<Door> ReadOnlyDoorsValue = DoorsValue.AsReadOnly();
        private static readonly ReadOnlyCollection<Lift> ReadOnlyLiftsValue = LiftsValue.AsReadOnly();
        private static readonly ReadOnlyCollection<TeslaGate> ReadOnlyTeslasValue = TeslasValue.AsReadOnly();
        private static readonly ReadOnlyCollection<Ragdoll> ReadOnlyRagdollsValue = RagdollsValue.AsReadOnly();

        private static readonly RaycastHit[] CachedFindParentRoomRaycast = new RaycastHit[1];

        private static SpawnpointManager spawnpointManager;


        public static int ActivatedGenerators => Generator079.mainGenerator.totalVoltage;

        public static Camera079[] Cameras => Scp079PlayerScript.allCameras;

        public static ReadOnlyCollection<Ragdoll> Ragdolls => ReadOnlyRagdollsValue;

        //public static ReadOnlyCollection<Room> Rooms
        //{
        //    get
        //    {
        //        if (RoomsValue.Count == 0)
        //        {
        //            List<GameObject> roomObjects = ListPool<GameObject>.Shared.Rent();

        //            // Get bulk of rooms.
        //            roomObjects.AddRange(GameObject.FindGameObjectsWithTag("Room"));

        //            // If no rooms were found, it means a plugin is trying to access this before the map is created.
        //            if (roomObjects.Count == 0)
        //            {
        //                ListPool<GameObject>.Shared.Return(roomObjects);
        //                throw new InvalidOperationException("Plugin is trying to access Rooms before they are created.");
        //            }

        //            // Add the pocket dimension since it is not tagged Room.
        //            const string PocketPath = "HeavyRooms/PocketWorld";
        //            var pocket = GameObject.Find(PocketPath);
        //            if (pocket == null)
        //                Log.Send($"[{typeof(Map).FullName}]: Pocket Dimension not found. The name or location in the game's hierarchy might have changed.", Discord.LogLevel.Error, ConsoleColor.DarkRed);
        //            else
        //                roomObjects.Add(pocket);

        //            // Add the surface since it is not tagged Room. Add it last so we can use it as a default room since it never changes.
        //            const string surfaceRoomName = "Outside";
        //            var surface = GameObject.Find(surfaceRoomName);
        //            if (surface == null)
        //                Log.Send($"[{typeof(Map).FullName}]: Surface not found. The name in the game's hierarchy might have changed.", Discord.LogLevel.Error, ConsoleColor.DarkRed);
        //            else
        //                roomObjects.Add(surface);

        //            foreach (var roomObject in roomObjects)
        //            {
        //                RoomsValue.Add(Room.CreateComponent(roomObject));
        //            }

        //            ListPool<GameObject>.Shared.Return(roomObjects);
        //        }

        //        return ReadOnlyRoomsValue;
        //    }
        //}

        //public static ReadOnlyCollection<Door> Doors
        //{
        //    get
        //    {
        //        if (DoorsValue.Count == 0)
        //        {
        //            DoorsValue.AddRange(Object.FindObjectsByType<Door>(FindObjectsSortMode.None));
        //            DoorTypeExtension.RegisterDoorTypesOnLevelLoad();
        //        }

        //        return ReadOnlyDoorsValue;
        //    }
        //}

        public static ReadOnlyCollection<Lift> Lifts
        {
            get
            {
                if (LiftsValue.Count == 0)
                    LiftsValue.AddRange(Object.FindObjectsByType<Lift>(FindObjectsSortMode.None));

                return ReadOnlyLiftsValue;
            }
        }
        public static ReadOnlyCollection<TeslaGate> TeslaGates
        {
            get
            {
                if (TeslasValue.Count == 0)
                    TeslasValue.AddRange(Object.FindObjectsByType<TeslaGate>(FindObjectsSortMode.None));

                return ReadOnlyTeslasValue;
            }
        }
        //public static Room FindParentRoom(GameObject objectInRoom)
        //{
        //    // Avoid errors by forcing Map.Rooms to populate when this is called.
        //    var rooms = Rooms;

        //    Room room = null;

        //    const string playerTag = "Player";

        //    // First try to find the room owner quickly.
        //    if (!objectInRoom.CompareTag(playerTag))
        //    {
        //        room = objectInRoom.GetComponentInParent<Room>();
        //    }
        //    else
        //    {
        //        // Check for Scp079 if it's a player
        //        var ply = Player.Get(objectInRoom);
        //        if (ply.Role == RoleType.Scp079)
        //        {
        //            // Raycasting doesn't make sence,
        //            // Scp079 position is constant,
        //            // let it be 'Outside' instead
        //            room = FindParentRoom(ply.ReferenceHub.scp079PlayerScript.currentCamera.gameObject);
        //        }
        //    }

        //    if (room == null)
        //    {
        //        // Then try for objects that aren't children, like players and pickups.
        //        Ray ray = new Ray(objectInRoom.transform.position, Vector3.down);

        //        if (Physics.RaycastNonAlloc(ray, CachedFindParentRoomRaycast, 10, 1 << 0, QueryTriggerInteraction.Ignore) == 1)
        //        {
        //            room = CachedFindParentRoomRaycast[0].collider.gameObject.GetComponentInParent<Room>();
        //        }
        //    }

        //    // Always default to surface transform, since it's static.
        //    // The current index of the 'Outsise' room is the last one
        //    if (room == null && rooms.Count != 0)
        //        room = rooms[rooms.Count - 1];

        //    return room;
        //}

        public static void Broadcast(ushort duration, string message,bool isMonospaced, bool isClearLast)
        {
            if (isClearLast)
            {
                ClearBroadcasts();
            }
            Server.Broadcast.RpcAddElement(message, duration, isMonospaced);
        }

        public static void ClearBroadcasts() => Server.Broadcast.RpcClearElements();

        public static Vector3 GetRandomSpawnPoint(this RoleType roleType)
        {
            GameObject randomPosition;

            if (spawnpointManager == null)
            {
                spawnpointManager = Object.FindFirstObjectByType<SpawnpointManager>();
            }

            randomPosition = spawnpointManager.GetRandomPosition(roleType);

            return randomPosition == null ? Vector3.zero : randomPosition.transform.position;
        }

        public static void ClearCache()
        {
            spawnpointManager = null;

            //RoomsValue.Clear();
            //DoorsValue.Clear();
            LiftsValue.Clear();
            TeslasValue.Clear();
        }
    }
}
