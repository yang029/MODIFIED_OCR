﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyTransformTool
{
    public class MyPhysicsCallbacks : MonoBehaviour
    {
        public static Action OnPrePhysics
        {
            get { return Provider._onPrePhysics; }
            set { Provider._onPrePhysics = value; }
        }

        public static Action OnPostPhysics
        {
            get { return Provider._onPostPhysics; }
            set { Provider._onPostPhysics = value; }
        }

        public static MyPhysicsCallbacks _instance;
        public static MyPhysicsCallbacks Provider
        {
            get
            {
                if (_instance == null)
                {
                    _instance = ConstructSingleton();
                }
                return _instance;
            }
        }

        private static MyPhysicsCallbacks ConstructSingleton()
        {
            GameObject parent = new GameObject("Physics Callbacks Provider");
            parent.transform.position = new Vector3(-10000F, -10000F, -10000F);

            GameObject trigger0 = new GameObject("OnPostPhysics Trigger 0");
            trigger0.transform.parent = parent.transform;
            trigger0.transform.localPosition = Vector3.zero;
            var body = trigger0.AddComponent<Rigidbody>();
            body.isKinematic = true;
            var box = trigger0.AddComponent<BoxCollider>();
            box.isTrigger = true;

            GameObject trigger1 = Instantiate<GameObject>(trigger0);
            trigger1.name = "OnPostPhysics Trigger 1";
            trigger1.transform.parent = parent.transform;
            trigger1.transform.localPosition = Vector3.zero;

            MyPhysicsCallbacks postPhysicsTrigger = trigger0.AddComponent<MyPhysicsCallbacks>();
            return postPhysicsTrigger;
        }

        private Action _onPrePhysics = () => { };
        private Action _onPostPhysics = () => { };

        void FixedUpdate()
        {
            _onPrePhysics();
        }

        void OnTriggerStay()
        {
            _onPostPhysics();
        }
    }

}

