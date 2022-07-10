using UnityEngine;
using Particle = UnityEngine.ParticleSystem.Particle;
using System.Collections.Generic;
using UnityEngine.Events;
using System;
/// <summary>
/// 增加了粒子系统常用的事件
/// 参考：https://forum.unity.com/threads/access-to-the-particle-system-lifecycle-events.328918/
/// </summary>
public class ParticleSystemEventListener : ComponentHelperBase<ParticleSystem>
{
    public Vector3Event onParticleBirthPos = new Vector3Event();//粒子特效生成的位置
    public ParticleEvent onParticleBirth = new ParticleEvent();
    public UnityEvent onParticleDie = new UnityEvent();
    public Transform tfAudioSource;

    private Particle[] _particles;
    private float _shortestTimeAlive = float.MaxValue;
    private List<float> _aliveParticlesRemainingTime = new List<float>();

    private void Awake()
    {
        _particles = new Particle[Comp.main.maxParticles];
    }
    private void LateUpdate()
    {
        TryBroadcastParticleDeath();
        if (Comp.particleCount == 0)
            return;

        var numParticlesAlive = Comp.GetParticles(_particles);
        float youngestParticleTimeAlive = float.MaxValue;
        var youngestParticles = GetYoungestParticles(numParticlesAlive, _particles, ref youngestParticleTimeAlive);
        if (_shortestTimeAlive > youngestParticleTimeAlive)
        {
            for (int i = 0; i < youngestParticles.Length; i++)
            {

                //位置
                Vector3 particlePos = youngestParticles[i].position;
                switch (Comp.main.simulationSpace)
                {
                    //从局部转变为世界坐标
                    case ParticleSystemSimulationSpace.Local:
                        particlePos = Comp.transform.TransformPoint(particlePos);
                        break;
                }
                onParticleBirthPos.Invoke(particlePos);
                onParticleBirth.Invoke(youngestParticles[i]);

                _aliveParticlesRemainingTime.Add(youngestParticles[i].remainingLifetime);
            }
        }
        _shortestTimeAlive = youngestParticleTimeAlive;
    }
    private void TryBroadcastParticleDeath()
    {
        for (int i = _aliveParticlesRemainingTime.Count - 1; i > -1; i--)
        {
            _aliveParticlesRemainingTime[i] -= Time.deltaTime;
            if (_aliveParticlesRemainingTime[i] <= 0)
            {
                _aliveParticlesRemainingTime.RemoveAt(i);
                onParticleDie.Invoke();
            }
        }
    }
    private Particle[] GetYoungestParticles(int numPartAlive, Particle[] particles, ref float youngestParticleTimeAlive)
    {
        var youngestParticles = new List<Particle>();
        for (int i = 0; i < numPartAlive; i++)
        {
            var timeAlive = particles[i].startLifetime - particles[i].remainingLifetime;
            if (timeAlive < youngestParticleTimeAlive)
            {
                youngestParticleTimeAlive = timeAlive;
            }
        }
        for (int i = 0; i < numPartAlive; i++)
        {
            var timeAlive = particles[i].startLifetime - particles[i].remainingLifetime;
            if (timeAlive == youngestParticleTimeAlive)
            {
                youngestParticles.Add(particles[i]);
            }
        }
        return youngestParticles.ToArray();
    }

    [Serializable]
    public class ParticleEvent : UnityEvent<Particle>
    {

    }

}
