# Audio y cinemáticas — Marco

Base: Unity 6000.3.23f1, URP 17.3 e Input System 1.20 del equipo. La escena original `Assets/Scenes/SampleScene.unity` y la configuración del proyecto se conservan.

## Tarea #17: audio espacial y reverberación

Escena de comprobación: `Assets/Marco/Scenes/AudioCinematicsLab.unity`.

- `PositionalAudio`: configura AudioSource 3D, atenuación, distancia, Doppler desactivado y envío a reverb.
- `SpatialAudioProfile`: perfiles editables para efectos y pasos; admite un AudioMixerGroup del proyecto.
- `ReverbPasillo.prefab` y `ReverbBano.prefab`: presets y radios editables. Las zonas son esféricas; ajustar su alcance al mapa real.
- `AudioPlayer`: identificador en la raíz del jugador; no agrega un segundo AudioListener.
- `DemoPlayerController`: recorrido de prueba compatible con el Input System del equipo. No reemplaza el controlador definitivo.

Para generar el laboratorio: menú Marco → Crear escena de audio y cinemáticas. Este comando pide guardar la escena abierta y genera únicamente recursos en Assets/Marco; no cambia las escenas de compilación ni el sistema de controles del equipo.

Controles: WASD, ratón, F linterna, Esc liberar cursor y clic para volver. Se incluyen ambientes y pasos sintetizados para comprobar técnicamente la acústica, pendientes de revisión auditiva y recursos finales.

No se incluyen assets del colegio ni el modelo The Wrapped. La geometría es un laboratorio funcional para revisar la tarea antes de contar con el mapa.

## Flujo de entrega

El trabajo se prepara localmente sin publicar ramas ni modificar main. El líder crea la rama de cada tarea; después se sincroniza main, se traen las ramas asignadas, se aplican los commits correspondientes, se verifica en Unity y se abre el PR dirigido a AlssnQu. Solo entonces se mueve a In review.
