using System;
using System.Collections.Generic;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

using WorldsGame.Camera;
using WorldsGame.Gamestates;
using WorldsGame.Models;
using WorldsGame.Models.Tools;
using WorldsGame.Network.Message;
using WorldsGame.Players;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Playing.Entities.Templates;
using WorldsGame.Playing.Items.Inventory;
using WorldsGame.Playing.NPCs;
using WorldsGame.Playing.Physics.Components;
using WorldsGame.Playing.Players.Entity;
using WorldsGame.Terrain;
using WorldsGame.Terrain.Blocks.Types;
using WorldsGame.Utils.EventMessenger;
using WorldsLib;

namespace WorldsGame.Playing.Players
{
    internal class Player : IDisposable
    {
        internal const int FRAMES_AMOUNT_TO_INTERPOLATE = 1000 / (ServerState.GAME_STEP_IN_MILLISECONDS * 4);
        private const float ROTATIONSPEED = 0.1f;

        internal readonly FirstPersonCamera _fpsCamera;
        private readonly ThirdPersonCamera _tpsCamera;
        private bool _isClientPlayer;

        private readonly int _worldDividerX, _worldDividerY, _worldDividerZ;

        private AudioListener _audioListener;

        internal AudioListener AudioListener
        {
            get
            {
                _audioListener.Position = Position;
                return _audioListener;
            }
        }

        internal bool IsFirstPerson { get; private set; }

        internal Vector3 LookVector { get; set; }

        internal PlayerTool Tool { get; set; }

        // That is for head tilting
        internal double HeadBob { get; set; }

        internal bool IsFlying { get; set; }

        internal World World { get; private set; }

        internal PlayerPhysics Physics { get; private set; }

        // From now on this should always be the center of the most bottom position
        internal Vector3 Position
        {
            get { return PlayerEntity.GetComponent<PositionComponent>().Position; }
            set { PlayerEntity.GetComponent<PositionComponent>().Position = value; }
        }

        internal Vector3 FacePosition
        {
            get { return Position + new Vector3(0, FaceHeight, 0); }
        }

        internal float YVelocity { get { return PlayerEntity.GetComponent<PhysicsComponent>().YVelocity; } }

        internal Vector3 CameraPosition
        {
            get
            {
                if (IsFirstPerson)
                {
                    return _fpsCamera.Position;
                }

                return _tpsCamera.Position;
            }
        }

        internal float LeftRightRotation
        {
            get { return PlayerEntity.GetComponent<ScaleAndRotateComponent>().LeftRightRotation; }
            set { PlayerEntity.GetComponent<ScaleAndRotateComponent>().LeftRightRotation = value; }
        }

        //        internal float UpDownRotation { get; set; }

        internal float UpDownRotation
        {
            get { return PlayerEntity.GetComponent<ScaleAndRotateComponent>().UpDownRotation; }
            set { PlayerEntity.GetComponent<ScaleAndRotateComponent>().UpDownRotation = value; }
        }

        internal Matrix CameraProjection
        {
            get
            {
                if (IsFirstPerson)
                {
                    return _fpsCamera.Projection;
                }

                return _tpsCamera.Projection;
            }
        }

        internal Matrix CameraView
        {
            get
            {
                if (IsFirstPerson)
                {
                    return _fpsCamera.View;
                }

                return _tpsCamera.View;
            }
        }

        internal Viewport CameraViewport
        {
            get
            {
                if (IsFirstPerson)
                {
                    return _fpsCamera.Viewport;
                }

                return _tpsCamera.Viewport;
            }
        }

        internal BoundingFrustum ViewFrustum
        {
            get
            {
                if (IsFirstPerson)
                {
                    return _fpsCamera.ViewFrustum;
                }
                return _tpsCamera.ViewFrustum;
            }
        }

        //        internal Vector3 LastPositionDelta { get; private set; }

        internal BlockType FootBlock
        {
            get { return World.GetBlock(Position); }
        }

        internal BlockType HeadBlock
        {
            get { return World.GetBlock(HeadPosition); }
        }

        internal Vector3 HeadPosition
        {
            get { return Position + new Vector3(0, Height, 0); }
        }

        public float Height { get; private set; }

        internal Vector3 DrawPosition
        {
            get
            {
                //TODO: WOrldDIVIDER и CHUNKSIZE можно перемножать заранее
                return new Vector3(
                Position.X - _worldDividerX * World.IndexMultiplier.X,
                Position.Y - _worldDividerY * World.IndexMultiplier.Y,
                Position.Z - _worldDividerZ * World.IndexMultiplier.Z);
            }
        }

        //        internal Vector3 VerticalVelocityAsVector3
        //        {
        //            get { return new Vector3(0, YVelocity, 0); }
        //        }

        internal SelectionBlock SelectionBlock { get; set; }

        internal bool UseSelectionBlockWithAir { get; set; }

        internal bool HideSelectionBlock { get; set; }

        internal PositionedBlock CurrentSelection
        {
            get
            {
                return SelectionBlock.CurrentSelection;
            }
        }

        internal PositionedBlock CurrentSelectedAdjacent
        {
            get { return SelectionBlock.CurrentSelectedAdjacent; }
        }

        private Vector3 DefaultPosition
        {
            get
            {
                return World.WorldType == WorldType.LocalWorld || World.WorldType == WorldType.NetworkWorld
                           ? new Vector3(World.ORIGIN.X, World.ORIGIN.Y, World.ORIGIN.Z)
                           : new Vector3(World.ORIGIN.X - 10, World.ORIGIN.Y, World.ORIGIN.Z - 1);
            }
        }

        internal BoundingBox BoundingBox { get; private set; }

        internal NetworkPlayerDescription PlayerDescription
        {
            get
            {
                return new NetworkPlayerDescription
                {
                    IsMovingForward = PlayerEntity.GetComponent<PhysicsComponent>().IsMovingForward,
                    IsMovingBackward = PlayerEntity.GetComponent<PhysicsComponent>().IsMovingBackward,
                    IsStrafingLeft = PlayerEntity.GetComponent<PhysicsComponent>().IsStrafingLeft,
                    IsStrafingRight = PlayerEntity.GetComponent<PhysicsComponent>().IsStrafingRight,

                    LeftRightRotation = LeftRightRotation,
                    UpDownRotation = UpDownRotation,

                    Position = Position,
                    Timestamp = NetTime.Now
                };
            }
        }

        // should be null if not server
        internal NetConnection Connection { get; set; }

        internal string Username { get; set; }

        internal byte ServerSlot { get; set; }

        private ClientPlayerActionManager _playerActionManager;

        internal ClientPlayerActionManager PlayerActionManager
        {
            get { return _playerActionManager; }
            set
            {
                _playerActionManager = value;
                Physics.PlayerActionManager = value;

                if (value != null)
                {
                    _playerActionManager.OnPlayerDidPrimaryAction += DoPrimaryAction;
                    _playerActionManager.OnPlayerDidSecondaryAction += DoSecondaryAction;
                    _playerActionManager.OnPlayerChangedCamera += ToggleCameraMode;
                }
            }
        }

        internal IMovementInterpolator MovementInterpolator { get; set; }

        internal Inventory Inventory { get; set; }

        internal event Action OnPlayerMovementBehaviourChanged = () => { };

        public bool IsPlayerPositionFound { get; set; }

        internal float FaceHeight { get; private set; }

        internal Entities.Entity PlayerEntity { get; private set; }

        internal bool IsClientPlayer
        {
            get { return _isClientPlayer; }
        }

        public bool IsDead { get; set; }

        internal Player(World world, Vector3 position, bool isClientPlayer = true)
        {
            IsFirstPerson = false;

            World = world;
            _isClientPlayer = isClientPlayer;

            if (IsClientPlayer)
            {
                _fpsCamera = new FirstPersonCamera(World.Graphics.Viewport, this);
                _tpsCamera = new ThirdPersonCamera(World.Graphics.Viewport, this);
            }
            //            Tool = new BlockRemover(this);

            Physics = new PlayerPhysics(this);

            SelectionBlock = new SelectionBlock(this);

            _worldDividerX = World.WORLD_DIVIDER * Chunk.SIZE.X;
            _worldDividerY = World.WORLD_DIVIDER * Chunk.SIZE.Y;
            _worldDividerZ = World.WORLD_DIVIDER * Chunk.SIZE.Z;

            if (World.IsNetworkWorld)
            {
                MovementInterpolator = new PlayerMovementInterpolator(this);
            }
            else
            {
                MovementInterpolator = new EmptyMovementInterpolator();
            }

            Inventory = new Inventory();

            if (!World.IsNetworkWorld)
            {
                _audioListener = new AudioListener();
            }

            PlayerEntity = ClientPlayerEntityTemplate.Instance.BuildEntity(World.EntityWorld, this, position);

            PlayerEntity.GetComponent<CharacterActorComponent>().OnAttributeChange += HealthMonitor;

            Tool = new PlayerTool(this);
        }

        internal void Initialize()
        {
            if (IsClientPlayer)
            {
                _fpsCamera.Initialize();
                _tpsCamera.Initialize();
            }

            Height = CharacterHelper.PlayerCharacter.MaxVertice.Y - CharacterHelper.PlayerCharacter.MinVertice.Y;

            FaceHeight = CharacterHelper.PlayerCharacter.FaceHeight;
            BoundingBox = new BoundingBox(CharacterHelper.PlayerCharacter.MinVertice,
                                          CharacterHelper.PlayerCharacter.MaxVertice);

            Messenger.On("PauseMenuStart", OnPauseMenuStart);
            Messenger.On("PauseMenuStop", OnPauseMenuStop);

            if (IsClientPlayer)
            {
                Messenger.On<float>("MouseDeltaXChange", OnMouseDXChange);
                Messenger.On<float>("MouseDeltaYChange", OnMouseDYChange);
            }
        }

        internal void InitializeInventory(List<InventoryItem> items, byte inventorySelectedSlot)
        {
            Inventory.SelectedSlot = inventorySelectedSlot;
            Inventory.RefillItems(items);

            // no commenting here, since there is no inventory action manager initialized yet
            ChangeItemInHand(Inventory.SelectedItem);
        }

        public void Respawn()
        {
            ClientPlayerEntityTemplate.Instance.ResetEntity(PlayerEntity);

            if (IsClientPlayer)
            {
                _fpsCamera.Initialize();
                _tpsCamera.Initialize();
            }

            // BUG This is not an intended behaviour, but it's better than to leave previous item in hand
            Tool.ChangeItem(null);

            PlayerEntity.IsActive = true;
        }

        private void OnPauseMenuStop()
        {
            if (_playerActionManager != null)
            {
                _playerActionManager.Enabled = true;
            }
        }

        private void OnPauseMenuStart()
        {
            if (_playerActionManager != null)
            {
                _playerActionManager.Enabled = false;
            }
        }

        private void OnMouseDXChange(float mouseDX)
        {
            LeftRightRotation = LeftRightRotation - ROTATIONSPEED * (mouseDX / 50);
        }

        private void OnMouseDYChange(float mouseDY)
        {
            UpDownRotation = UpDownRotation - ROTATIONSPEED * (mouseDY / 50);

            // Locking camera rotation vertically between +/- 180 degrees
            float newPosition = UpDownRotation - ROTATIONSPEED * (mouseDY / 50);

            if (newPosition < -1.55f)
                newPosition = -1.55f;
            else if (newPosition > 1.55f)
                newPosition = 1.55f;

            UpDownRotation = newPosition;
            // End of locking
        }

        internal void DoPlayerAction(PlayerNetworkActionType actionType)
        {
            if (actionType == PlayerNetworkActionType.PrimaryAction)
            {
                DoPrimaryAction();
            }
            else if (actionType == PlayerNetworkActionType.SecondaryAction)
            {
                DoSecondaryAction();
            }
        }

        private void DoSecondaryAction()
        {
            Tool.DoSecondaryAction();
        }

        private void DoPrimaryAction()
        {
            Tool.DoPrimaryAction();
        }

        private void ToggleCameraMode()
        {
            IsFirstPerson = !IsFirstPerson;

            PlayerEntity.GetBehaviour<PlayerBehaviour>().OnPlayerCameraToggle(PlayerEntity, IsFirstPerson);
        }

        internal void Update(GameTime gameTime)
        {
            FindYPositionOnStart();

            if (!World.IsServerWorld && IsClientPlayer)
            {
                if (IsFirstPerson)
                {
                    _fpsCamera.Update(gameTime);
                }
                else
                {
                    _tpsCamera.Update(gameTime);
                }
            }

            //            Vector3 previousPosition = Position;
            //            Physics.Update(gameTime);
            //            LastPositionDelta = Position - previousPosition;

            MovementInterpolator.Interpolate();

            UpdateLookVector();

            SelectionBlock.Update();
        }

        private void FindYPositionOnStart()
        {
            if (!IsPlayerPositionFound)
            {
                for (int i = 0; i < 10; i++)
                {
                    PlayerEntity.GetComponent<PositionComponent>().YPosition += 0.5f;
                    IsPlayerPositionFound = !HeadBlock.IsSolid;

                    if (IsPlayerPositionFound)
                    {
                        break;
                    }
                }
            }
        }

        internal void Update(float totalSeconds, NetworkPlayerDescription playerDescription)
        {
            Physics.Update(totalSeconds, playerDescription);
            NetworkUpdateLookVector(playerDescription);
            SelectionBlock.Update();
        }

        private void NetworkUpdateLookVector(NetworkPlayerDescription playerDescription)
        {
            LeftRightRotation = playerDescription.LeftRightRotation;
            UpDownRotation = playerDescription.UpDownRotation;
            UpdateLookVector();
        }

        private void UpdateLookVector()
        {
            LookVector = IsFirstPerson ? _fpsCamera.LookVector : _tpsCamera.LookVector;

            LookVector.Normalize();
        }

        internal Vector3i GetCurrentChunkIndex()
        {
            float positionX = Position.X;
            float positionY = Position.Y;
            float positionZ = Position.Z;

            var currentXChunk = (int)(positionX / Chunk.SIZE.X);
            var currentYChunk = (int)(positionY / Chunk.SIZE.Y);
            var currentZChunk = (int)(positionZ / Chunk.SIZE.Z);

            if (positionX < 0)
            {
                currentXChunk -= 1;
            }
            if (positionY < 0)
            {
                currentYChunk -= 1;
            }
            if (positionZ < 0)
            {
                currentZChunk -= 1;
            }

            return new Vector3i(currentXChunk, currentYChunk, currentZChunk);
        }

        internal Chunk GetCurrentChunk()
        {
            return World.GetChunk(GetCurrentChunkIndex());
        }

        internal void SetIntoDefaultPosition()
        {
            //            Position = DefaultPosition;

            //            UpDownRotation = 0;
            //            LeftRightRotation = -MathHelper.PiOver2;
        }

        internal bool IsChunkOutOfView(Chunk chunk)
        {
            Vector3i currentChunkIndex = GetCurrentChunkIndex();
            return chunk.Index.X < currentChunkIndex.X - World.ChunkGenerationRange ||
                   chunk.Index.X > currentChunkIndex.X + World.ChunkGenerationRange ||
                   chunk.Index.Z < currentChunkIndex.Z - World.ChunkGenerationRange ||
                   chunk.Index.Z > currentChunkIndex.Z + World.ChunkGenerationRange ||
                   chunk.Index.Y < currentChunkIndex.Y - World.CHUNK_GENERATION_RANGE_Y ||
                   chunk.Index.Y > currentChunkIndex.Y + World.CHUNK_GENERATION_RANGE_Y;
        }

        // Used in multiplayer. Returns updated position from server data
        internal void ReconcilePosition(float totalSeconds, NetworkPlayerDescription playerDescription)
        {
            if (playerDescription.JumpOccured)
            {
                Physics.Jump(quiet: true);
                return;
            }

            Update(totalSeconds, playerDescription);
        }

        /// <summary>
        /// Used to set up variables needed for interpolation after server update, also starts interpolation. Multiplayer only.
        /// </summary>
        /// <param name="newPosition">Position to interpolate to</param>
        internal void SetupInterpolation(Vector3 newPosition)
        {
            MovementInterpolator.UpdateInterpolationData(newPosition);
        }

        // This is Hardcode 1986
        [Obsolete]
        internal void SetAdderBlockType(int key)
        {
            //                        ((BlockAdder)SecondaryAction).SetType(key);
        }

        internal void AddItem(string name, int quantity)
        {
            Inventory.AddItem(name, quantity);
        }

        internal void ChangeItemInHand(InventoryItem item)
        {
            if (item != null)
            {
                Tool.ChangeItem(item.Name);
            }
            else
            {
                Tool.ChangeItem(null);
            }
        }

        private void HealthMonitor(Entities.Entity owner, string attribute, float value)
        {
            if (attribute == "health" && value <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Inventory.DropEverything(this);

            Messenger.Invoke("PlayerDied", this);
        }

        internal void ToggleMovementBehaviourChange()
        {
            IsFlying = !IsFlying;
            Physics.ToggleMovementBehaviourChange();

            OnPlayerMovementBehaviourChanged();
        }

        public void Dispose()
        {
            Tool.Dispose();

            Physics.Dispose();

            Messenger.Off("PauseMenuStart", OnPauseMenuStart);
            Messenger.Off("PauseMenuStop", OnPauseMenuStop);

            if (IsClientPlayer)
            {
                Messenger.Off<float>("MouseDeltaXChange", OnMouseDXChange);
                Messenger.Off<float>("MouseDeltaYChange", OnMouseDYChange);
            }

            if (_playerActionManager != null)
            {
                // This is doubled in ClientPlayerManager for some reason, maybe for other players, idk
                _playerActionManager.OnPlayerDidPrimaryAction -= DoPrimaryAction;
                _playerActionManager.OnPlayerDidSecondaryAction -= DoSecondaryAction;
            }
        }
    }
}