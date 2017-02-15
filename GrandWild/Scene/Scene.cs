using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmNet;

namespace org.flamerat.GrandWild.Scene {
    public class Scene {
        public List<GrandWildKernel> BelongingKernels = new List<GrandWildKernel>();

        public Rendering.IRenderable[] RequiredRenderables;

        private List<Entity.Entity> _Entities = new List<Entity.Entity>();
        public List<Entity.Entity> Entities {
            get {
                return _Entities;
            }
            set {
                foreach(var entity in _Entities) {
                    entity.BelongingScenes.Remove(this);
                    if (entity.GetType().GetInterfaces().Contains(typeof(ITimerBased))) {
                        _MainTimer.OnTimeTick -= ((ITimerBased)entity).TimeTickBehavior;
                    }
                }
                _Entities = value;
                foreach (var entity in _Entities) {
                    entity.BelongingScenes.Add(this);
                    if (entity.GetType().GetInterfaces().Contains(typeof(ITimerBased))) {
                        _MainTimer.OnTimeTick += ((ITimerBased)entity).TimeTickBehavior;
                    }
                }
            }
        }
        public void AddEntity(Entity.Entity entity) {
            _Entities.Add(entity);
            entity.BelongingScenes.Add(this);
            if (entity.GetType().GetInterfaces().Contains(typeof(ITimerBased))) {
                _MainTimer.OnTimeTick -= ((ITimerBased)entity).TimeTickBehavior;
            }
        }
        public void AddEntities(IEnumerable<Entity.Entity> entities) {
            foreach (var entity in entities) AddEntity(entity);
        }
        
        public void RemoveEntity(Entity.Entity entity){
            _Entities.Remove(entity);
            entity.BelongingScenes.Remove(this);
            if (entity.GetType().GetInterfaces().Contains(typeof(ITimerBased))) {
                _MainTimer.OnTimeTick -= ((ITimerBased)entity).TimeTickBehavior;
            }
        }
        public void RemoveEntities(IEnumerable<Entity.Entity> entities) {
            foreach (var entity in entities) RemoveEntity(entity);
        }








        private GpuBufferMemory vertexBufferMemory;
        private GpuBufferMemory indexBufferMemory;
        private GpuImageMemory textureMemory;

        public void SendRequiredGpuObjectToGpu(Vulkan.PhysicalDevice physicalDevice,Vulkan.Device device) {
            if (vertexBufferMemory != null) {
                vertexBufferMemory.Dispose();
                vertexBufferMemory = null;
            }
            if (indexBufferMemory != null) {
                indexBufferMemory.Dispose();
                indexBufferMemory = null;
            }
            if (textureMemory != null) {
                textureMemory.Dispose();
                textureMemory = null;
            }

            var allRenderables = RequiredRenderables.Union(Entities);
            var allVertexBuffers = allRenderables.SelectMany(renderable => renderable.GetVertexBuffers()).Distinct().ToArray();
            var allIndexBuffers = allRenderables.SelectMany(renderable => renderable.GetIndexBuffers()).Distinct().ToArray();
            var allTextures = allRenderables.SelectMany(renderable => renderable.GetTextureImages()).Distinct().ToArray();

            vertexBufferMemory = new GpuBufferMemory(physicalDevice, device, allVertexBuffers);
            indexBufferMemory = new GpuBufferMemory(physicalDevice, device, allIndexBuffers);
            textureMemory = new GpuImageMemory(physicalDevice, device, allTextures);
        }

        public struct SceneInfo {
            public float globalLightStrength;
            public vec4 globalLightDirection;
            public vec4 globalLightColor;
            public vec4 fogColor;
            public float fogDensity;
        }

        public struct CameraInfo {
            public vec3 Position;
            public float XRotation;
            public float YRotation;
            public float ZRotation;
            public float Near;
            public float Far;
            public float FieldOfView;
            public bool IsOrthoMode;
            public float OrthoSpan; //could be the width or the height, depending on the aspect ratio mode
            public mat4 ViewProjectionMatrix {
                get {
                    mat4 result = new mat4(1.0F);
                    glm.translate(result, new vec3(-Position.x, -Position.y, -Position.z));
                    glm.rotate(result, -glm.radians(XRotation), new vec3(1, 0, 0));
                    glm.rotate(result, -glm.radians(YRotation), new vec3(0, 1, 0));
                    glm.rotate(result, -glm.radians(ZRotation), new vec3(0, 0, 1));
                    if (IsOrthoMode) {
                        result = glm.ortho(
                            left: -OrthoSpan / 2, right: OrthoSpan / 2,
                            bottom: OrthoSpan / 2, top: -OrthoSpan / 2, 
                            zNear: Near, zFar: Far
                        );
                    }else {
                        result = glm.perspective(fovy: FieldOfView, aspect: 1.0F, zNear: Near, zFar: Far)*result;
                    }
                    return result;
                }
            }
        }
        public struct ScreenInfo {
            public float AspectRatio;
            public void SetAspectRatio(float ratio) {
                AspectRatio = ratio;
            }
            public void SetAspectRatio(float x,float y) {
                AspectRatio = x / y;
            }
            public enum AspectRatioMode {
                FixedHeight,
                FixedWidth
            }
            public AspectRatioMode ScreenAspectRatioMode;
            public mat4 ClipMatrix {
                get {
                    switch (ScreenAspectRatioMode) {
                        case AspectRatioMode.FixedHeight:
                            return glm.scale(new mat4(1.0F), new vec3(1.0F / AspectRatio, 1, 1));
                        case AspectRatioMode.FixedWidth:
                            return glm.scale(new mat4(1.0F), new vec3(1, AspectRatio, 1));
                        default:
                            return glm.scale(new mat4(1.0F), new vec3(1.0F / AspectRatio, 1, 1));
                    }
                    
                }
            }
        }

        public SceneInfo Info = new SceneInfo {
            globalLightColor = new vec4(1, 1, 1, 1),
            globalLightDirection = glm.normalize(new vec4(1, -1, -1, 0)),
            globalLightStrength = 0.2F,
            fogColor = new vec4(0, 0, 0, 1),
            fogDensity = 0.5F
        };
        private CameraInfo _Camera;
        private ScreenInfo _Screen;
        public void SetAspectRatio(float x,float y) {
            _Screen.SetAspectRatio(x, y);
        }
        public mat4 CameraVpcMatric {
            get {
                return _Screen.ClipMatrix * _Camera.ViewProjectionMatrix;
            }
        }

        public delegate void OnMoveEvent(Scene sender, vec3 oldPosition, vec3 newPosition);
        public event OnMoveEvent OnMove;
        public void MoveTo(vec3 position) {
            var oldPosition = _Camera.Position;
            _Camera.Position = position;
            OnMove(this, oldPosition, _Camera.Position);
        }
        public void MoveFor(vec3 relativePosition) {
            var oldPosition = _Camera.Position;
            _Camera.Position += relativePosition;
            OnMove(this, oldPosition, _Camera.Position);
        }
        public void MoveForSelfCoordinate(float right, float up, float forward) {
            var selfMovement = new vec4(right, up, -forward,1);
            mat4 rotateMatrix = new mat4(1);
            glm.rotate(rotateMatrix, glm.radians(_Camera.XRotation), new vec3(1, 0, 0));
            glm.rotate(rotateMatrix, glm.radians(_Camera.YRotation), new vec3(0, 1, 0));
            glm.rotate(rotateMatrix, glm.radians(_Camera.ZRotation), new vec3(0, 0, 1));
            selfMovement = rotateMatrix * selfMovement;
            MoveFor(new vec3(selfMovement));
        }

        public delegate void OnRotateEvent(Scene sender, float oldXR, float oldYR, float oldZR, float newXR, float newYR, float newZR);
        public event OnRotateEvent OnRotate;
        public void RotateTo(float xr, float yr, float zr) {
            var oldXR = _Camera.XRotation;
            var oldYR = _Camera.YRotation;
            var oldZR = _Camera.ZRotation;
            _Camera.XRotation = xr;
            _Camera.YRotation = yr;
            _Camera.ZRotation = zr;
            OnRotate(this, oldXR, oldYR, oldZR, _Camera.XRotation, _Camera.YRotation, _Camera.ZRotation);
            if (_Camera.XRotation > 180) _Camera.XRotation -= 360;
            if (_Camera.YRotation > 180) _Camera.YRotation -= 360;
            if (_Camera.ZRotation > 180) _Camera.ZRotation -= 360;
            if (_Camera.XRotation < -180) _Camera.XRotation += 360;
            if (_Camera.YRotation < -180) _Camera.YRotation += 360;
            if (_Camera.ZRotation < -180) _Camera.ZRotation += 360;
        }
        public void RotateFor(float xr, float yr, float zr) {
            var oldXR = _Camera.XRotation;
            var oldYR = _Camera.YRotation;
            var oldZR = _Camera.ZRotation;
            _Camera.XRotation += xr;
            _Camera.YRotation += yr;
            _Camera.ZRotation += zr;
            OnRotate(this, oldXR, oldYR, oldZR, _Camera.XRotation, _Camera.YRotation, _Camera.ZRotation);
            if (_Camera.XRotation > 180) _Camera.XRotation -= 360;
            if (_Camera.YRotation > 180) _Camera.YRotation -= 360;
            if (_Camera.ZRotation > 180) _Camera.ZRotation -= 360;
            if (_Camera.XRotation < -180) _Camera.XRotation += 360;
            if (_Camera.YRotation < -180) _Camera.YRotation += 360;
            if (_Camera.ZRotation < -180) _Camera.ZRotation += 360;
        }
        public void RotateForSelfCoordinate(float pan, float pitch, float tilt) {
            throw new NotImplementedException();
        }

        public delegate void OnRenderEvent(Scene sender);
        public event OnRenderEvent OnBeforeRender;
        public event OnRenderEvent OnAfterRender;
        public event OnRenderEvent OnGetFocus;
        public event OnRenderEvent OnLoseFocus;

        public void BeforeRenderBehavior() { OnBeforeRender(this); }
        public void AfterRenderBehavior() { OnAfterRender(this); }

        public void GetFocusBehavior() { OnGetFocus(this); }
        public void LoseFocusBehavior() { OnLoseFocus(this); }


        public delegate void OnCreateEvent(Scene sender);
        public event OnCreateEvent OnCreate;
        public Scene() {
            OnCreate(this);
            _Camera.FieldOfView = 85.0F;
            _Camera.Near = 0.1F;
            _Camera.Far = 1000.0F;
            _Camera.IsOrthoMode = false;

            _CameraTimer.OnTimeTick += _OnCameraTick;
            _CameraTimer.Start();

            if (this.GetType().GetInterfaces().Contains(typeof(ITimerBased))) {
                _MainTimer.OnTimeTick += ((ITimerBased)this).TimeTickBehavior;
            }
        }

        private void _OnCameraTick(Timer timer) {
            MoveForSelfCoordinate(
                TargetWalkSpeedRight * (float)timer.ActualInteval.TotalSeconds,
                TargetWalkSpeedUp * (float)timer.ActualInteval.TotalSeconds, 
                TargetWalkSpeedForward*(float)timer.ActualInteval.TotalSeconds
            );
            RotateFor(
                TargetXRotateSpeed * (float)timer.ActualInteval.TotalSeconds, 
                TargetYRotateSpeed * (float)timer.ActualInteval.TotalSeconds, 
                TargetZRotateSpeed * (float)timer.ActualInteval.TotalSeconds
            );
        }

        public delegate void OnDestroyEvent(Scene sender);
        public event OnDestroyEvent OnDestroy;
        ~Scene() {
            OnDestroy(this);
        }

        private Timer _MainTimer = new Timer(canSkipTick: true, targetInteval: new TimeSpan(0, 0, 0, 0, 50));
        public void StartMainTimer() {
            _MainTimer.Start();
        }
        public void StopMainTimer() {
            _MainTimer.Stop();
        }
        public void SetMainTimerInteval(TimeSpan inteval) {
            _MainTimer.TargetInteval = inteval;
        }
        public void SetMainTimerSpeed(float speedScale) {
            _MainTimer.SpeedScale = speedScale;
        }

        private Timer _CameraTimer = new Timer(canSkipTick: true, targetInteval: new TimeSpan(0, 0, 0, 0, 10));
        public volatile float TargetWalkSpeedForward=0.0F;
        public volatile float TargetWalkSpeedRight = 0.0F;
        public volatile float TargetWalkSpeedUp = 0.0F;
        public volatile float TargetXRotateSpeed = 0.0F;
        public volatile float TargetYRotateSpeed = 0.0F;
        public volatile float TargetZRotateSpeed = 0.0F;

    }
}
