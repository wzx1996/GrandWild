using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmNet;
using Vulkan;

namespace org.flamerat.GrandWild.Entity {
    class Entity: Rendering.Structure {
        public delegate void OnMoveEvent(Entity sender, vec3 oldPosition, vec3 newPosition);
        public event OnMoveEvent OnMove;
        public void MoveTo(vec3 position) {
            var oldPosition = _Position;
            _Position = position;
            OnMove(this,oldPosition,_Position);
        }
        public void MoveFor(vec3 relativePosition) {
            var oldPosition = _Position;
            _Position += relativePosition;
            OnMove(this, oldPosition, _Position);
        }

        public delegate void OnRotateEvent(Entity sender, float oldXR, float oldYR, float oldZR,float newXR,float newYR,float newZR);
        public event OnRotateEvent OnRotate;
        public void RotateTo(float xr,float yr,float zr) {
            var oldXR = _XRotation;
            var oldYR = _YRotation;
            var oldZR = _ZRotation;
            _XRotation = xr;
            _YRotation = yr;
            _ZRotation = zr;
            OnRotate(this, oldXR, oldYR, oldZR, _XRotation, _YRotation, _ZRotation);
            if (_XRotation > 180) _XRotation -= 360;
            if (_XRotation > 180) _YRotation -= 360;
            if (_XRotation > 180) _ZRotation -= 360;
            if (_XRotation < -180) _XRotation += 360;
            if (_XRotation < -180) _YRotation += 360;
            if (_XRotation < -180) _ZRotation += 360;
        }
        public void RotateFor(float xr,float yr,float zr) {
            var oldXR = _XRotation;
            var oldYR = _YRotation;
            var oldZR = _ZRotation;
            _XRotation += xr;
            _YRotation += yr;
            _ZRotation += zr;
            OnRotate(this, oldXR, oldYR, oldZR, _XRotation, _YRotation, _ZRotation);
            if(_XRotation > 180) _XRotation -= 360;
            if (_XRotation > 180) _YRotation -= 360;
            if (_XRotation > 180) _ZRotation -= 360;
            if (_XRotation < -180) _XRotation += 360;
            if (_XRotation < -180) _YRotation += 360;
            if (_XRotation < -180) _ZRotation += 360;
        }

        public delegate void OnDrawEvent(Entity sender);
        public event OnDrawEvent OnBeforeDraw;
        public event OnDrawEvent OnAfterDraw;

        public override void Draw(CommandBuffer commandBuffer) {
            OnBeforeDraw(this);
            Draw(commandBuffer,_ModelMatrix);
            OnAfterDraw(this);
        }


        public delegate void OnCreateEvent(Entity sender);
        public event OnCreateEvent OnCreate;
        public Entity() {
            OnCreate(this);
        }


        public delegate void OnDestroyEvent(Entity sender);
        public event OnDestroyEvent OnDestroy;
        ~Entity() {
            OnDestroy(this);
        }

        protected vec3 _Position;
        protected float _Scale;
        protected float _XRotation;
        protected float _YRotation;
        protected float _ZRotation;
        protected mat4 _ModelMatrix {
            get {
                mat4 result=new mat4(1);
                glm.scale(result, new vec3(_Scale));
                glm.rotate(result, glm.degrees(_XRotation), new vec3(1, 0, 0));
                glm.rotate(result, glm.degrees(_YRotation), new vec3(0, 1, 0));
                glm.rotate(result, glm.degrees(_ZRotation), new vec3(0, 0, 1));
                glm.translate(result, _Position);
                return result;
            }
        }
        protected new SubPart[] _SubParts {
            get { return base._SubParts; }
            set { base._SubParts = value; }
        }
    }
}
