using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmNet;

namespace org.flamerat.GrandWild.Scene {
    class Scene {
        public Rendering.IRenderable[] RequiredRenderables;
        public List<Entity.Entity> Entities;
        public struct SceneInfo {
            public float globalLightStrength;
            public vec4 globalLightDirection;
            public vec4 globalLightColor;
            public vec4 fogColor;
            public float fogDensity;
        }
        public struct UniformBufferCameraInfo {
            public mat4 ViewProjectionClip;
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
                    glm.rotate(result, glm.radians(XRotation), new vec3(1, 0, 0));
                    glm.rotate(result, glm.radians(YRotation), new vec3(0, 1, 0));
                    glm.rotate(result, glm.radians(ZRotation), new vec3(0, 0, 1));
                    if (IsOrthoMode) {
                        result = glm.ortho(
                            left: -OrthoSpan / 2, right: OrthoSpan / 2,
                            bottom: OrthoSpan / 2, top: -OrthoSpan / 2, 
                            zNear: Near, zFar: Far
                        );
                    }else {
                        result = glm.perspective(fovy: FieldOfView, aspect: 1.0F, zNear: Near, zFar: Far)*result;
                    }
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

        private CameraInfo _Camera;
        private mat4 _Clip = glm.scale(new mat4(1.0F),new vec3(9.0F/16.0F,1,1));

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
        public void MoveForSelfCoordinate(float forward, float right, float up) {

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
            if (_Camera.XRotation > 180) _Camera.YRotation -= 360;
            if (_Camera.XRotation > 180) _Camera.ZRotation -= 360;
            if (_Camera.XRotation < -180) _Camera.XRotation += 360;
            if (_Camera.XRotation < -180) _Camera.YRotation += 360;
            if (_Camera.XRotation < -180) _Camera.ZRotation += 360;
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
            if (_Camera.XRotation > 180) _Camera.YRotation -= 360;
            if (_Camera.XRotation > 180) _Camera.ZRotation -= 360;
            if (_Camera.XRotation < -180) _Camera.XRotation += 360;
            if (_Camera.XRotation < -180) _Camera.YRotation += 360;
            if (_Camera.XRotation < -180) _Camera.ZRotation += 360;
        }
        public void RotateForSelfCoordinate(float pan, float pitch, float tilt) {

        }

        public delegate void OnRenderEvent(Scene sender);
        public event OnRenderEvent OnBeforeRender;
        public event OnRenderEvent OnAfterRender;
        public event OnRenderEvent OnSetFocus;
        public event OnRenderEvent OnLoseFocus;


        public delegate void OnCreateEvent(Scene sender);
        public event OnCreateEvent OnCreate;
        public Scene() {
            OnCreate(this);
        }


        public delegate void OnDestroyEvent(Scene sender);
        public event OnDestroyEvent OnDestroy;
        ~Scene() {
            OnDestroy(this);
        }
    }
}
