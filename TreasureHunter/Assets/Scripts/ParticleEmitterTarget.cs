using UnityEngine;
using System.Collections;

public class ParticleEmitterTarget : MonoBehaviour
{
  public GameObject target;
  private Transform targetTransform;
  public float speed;
  private ParticleSystem pe;

  void Start()
  {
    pe = gameObject.GetComponent<ParticleSystem>();
    targetTransform = GameManager.Instance.player.transform;
  }


  void Update()
  {
    // Extract copy
    ParticleSystem.Particle[] emittedParticles = new ParticleSystem.Particle[pe.particleCount];
    int numParticlesAlive = pe.GetParticles(emittedParticles);

    // Do changes
    for(int i = 0; i < numParticlesAlive; i++)
    {
      emittedParticles[i].rotation += 10f;
      
      Vector3 direction = (targetTransform.position - emittedParticles[i].position);
      direction.Normalize();
      emittedParticles[i].position += (direction * Time.deltaTime * 10f);

      if(Vector3.Distance(targetTransform.position, emittedParticles[i].position) < 0.4f)
      {
        emittedParticles[i].position = new Vector3(500f, 500f, 500f);
      }
    }
    
    // Reassign back to emitter
    pe.SetParticles(emittedParticles, numParticlesAlive);
  }
}
