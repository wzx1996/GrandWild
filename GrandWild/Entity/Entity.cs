﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmNet;
using Vulkan;

namespace org.flamerat.GrandWild.Entity {
    public class Entity: Rendering.Structure {
        public List<Scene.Scene> BelongingScenes = new List<Scene.Scene>();
        public delegate void OnMoveEvent(Entity sender, vec3 oldPosition, vec3 newPosition);
        public event OnMoveEvent OnMove;
        public void MoveTo(vec3 position) {
            var oldPosition = Position;
            Position = position;
            OnMove?.Invoke(this,oldPosition,Position);
        }
        public void MoveFor(vec3 relativePosition) {
            var oldPosition = Position;
            Position += relativePosition;
            OnMove?.Invoke(this, oldPosition, Position);
        }
        public void MoveForSelfCoordinate(float forward,float right,float up) {
            var selfMovement = new vec4(right, up, -forward, 0);
            mat4 rotateMatrix = new mat4(1);
            rotateMatrix=glm.rotate(glm.radians(XRotation), new vec3(1, 0, 0))*rotateMatrix;
            rotateMatrix=glm.rotate(glm.radians(YRotation), new vec3(0, 1, 0)) * rotateMatrix;
            rotateMatrix=glm.rotate(glm.radians(ZRotation), new vec3(0, 0, 1)) * rotateMatrix;
            selfMovement = rotateMatrix * selfMovement;
            MoveFor(new vec3(selfMovement));
        }

        public delegate void OnRotateEvent(Entity sender, float oldXR, float oldYR, float oldZR,float newXR,float newYR,float newZR);
        public event OnRotateEvent OnRotate;
        public void RotateTo(float xr,float yr,float zr) {
            var oldXR = XRotation;
            var oldYR = YRotation;
            var oldZR = ZRotation;
            XRotation = xr;
            YRotation = yr;
            ZRotation = zr;
            OnRotate?.Invoke(this, oldXR, oldYR, oldZR, XRotation, YRotation, ZRotation);
            if (XRotation > 180) XRotation -= 360;
            if (YRotation > 180) YRotation -= 360;
            if (ZRotation > 180) ZRotation -= 360;
            if (XRotation < -180) XRotation += 360;
            if (YRotation < -180) YRotation += 360;
            if (ZRotation < -180) ZRotation += 360;
        }
        public void RotateFor(float xr,float yr,float zr) {
            var oldXR = XRotation;
            var oldYR = YRotation;
            var oldZR = ZRotation;
            XRotation += xr;
            YRotation += yr;
            ZRotation += zr;
            OnRotate?.Invoke(this, oldXR, oldYR, oldZR, XRotation, YRotation, ZRotation);
            if(XRotation > 180) XRotation -= 360;
            if (YRotation > 180) YRotation -= 360;
            if (ZRotation > 180) ZRotation -= 360;
            if (XRotation < -180) XRotation += 360;
            if (YRotation < -180) YRotation += 360;
            if (ZRotation < -180) ZRotation += 360;
        }
        public void RotateForSelfCoordinate(float pan,float pitch,float tilt) {
            throw new NotImplementedException();
        }

        public delegate void OnDrawEvent(Entity sender);
        public event OnDrawEvent OnBeforeDraw;
        public event OnDrawEvent OnAfterDraw;

        public override void Draw(CommandBuffer commandBuffer) {
            OnBeforeDraw?.Invoke(this);
            Draw(commandBuffer,_ModelMatrix);
            OnAfterDraw?.Invoke(this);
        }


        public delegate void OnCreateEvent(Entity sender);
        public event OnCreateEvent OnCreate;
        public Entity() {
            OnCreate?.Invoke(this);
        }


        public delegate void OnDestroyEvent(Entity sender);
        public event OnDestroyEvent OnDestroy;
        ~Entity() {
            OnDestroy?.Invoke(this);
        }

        public vec3 Position { get; private set; }
        public float Scale {
            get { return _Scale; }
            set { if (value > 0) _Scale = value; }
        }
        private float _Scale = 1.0F;
        public float XRotation { get; private set; }
        public float YRotation { get; private set; }
        public float ZRotation { get; private set; }
        private mat4 _ModelMatrix {
            get {
                mat4 result=new mat4(1);
                result=glm.scale(mat4.identity(), new vec3(_Scale))*result;
                result=glm.rotate(glm.radians(XRotation), new vec3(1, 0, 0))*result;
                result=glm.rotate(glm.radians(YRotation), new vec3(0, 1, 0))*result;
                result=glm.rotate(glm.radians(ZRotation), new vec3(0, 0, 1))*result;
                result=glm.translate(mat4.identity(), Position)*result;
                return result;
            }
        }

    }

}
