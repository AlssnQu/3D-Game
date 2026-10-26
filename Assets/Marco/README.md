# Audio espacial — Marco · Tarea #17

Base: Unity 6000.3.23f1, URP 17.3 e Input System 1.20 del equipo. La escena original `Assets/Scenes/SampleScene.unity` y la configuración del proyecto se conservan.

## Tarea #17: audio espacial y reverberación

Escena de comprobación: `Assets/Marco/Scenes/AudioCinematicsLab.unity`.

- `PositionalAudio`: configura AudioSource 3D, atenuación, distancia, Doppler desactivado y envío a reverb.
- `SpatialAudioProfile`: perfiles editables para efectos y pasos; admite un AudioMixerGroup del proyecto.
- `ReverbPasillo.prefab` y `ReverbBano.prefab`: presets y radios editables. Las zonas son esféricas; ajustar su alcance al mapa real.
- `AudioPlayer`: identificador en la raíz del jugador; no agrega un segundo AudioListener.
- `DemoPlayerController`: recorrido de prueba compatible con el Input System del equipo. No reemplaza el controlador definitivo.

Para generar el laboratorio: menú Marco → Crear escena de audio y cinemáticas. Este comando pide guardar la escena abierta y genera únicamente recursos en Assets/Marco; no cambia las escenas de compilación ni el sistema de controles del equipo.

Controles: WASD, ratón, F linterna, Esc liberar cursor y clic para volver. Los ambientes propios se combinan con pasos, metal, crujidos, bisagras y agua de una paleta CC0 documentada en Audio/Curated/CREDITS.md. La mezcla requiere revisión auditiva en el mapa final.

No se incluyen assets del colegio ni el modelo The Wrapped. La geometría es un laboratorio funcional para revisar la tarea antes de contar con el mapa.

## Flujo de entrega

El trabajo se prepara localmente sin publicar ramas ni modificar main. El líder crea la rama de cada tarea; después se sincroniza main, se traen las ramas asignadas, se aplican los commits correspondientes, se verifica en Unity y se abre el PR dirigido a AlssnQu. Solo entonces se mueve a In review.

## Verificación reproducible

En Unity cerrado, ejecutar el editor en batch con `-executeMethod LoQueCallaste.Editor.SpatialAudioSetup.VerifySpatialScene -quit` para revisar la configuración. Para comprobar reproducción y reverberación en Play: `-batchmode -nographics -executeMethod LoQueCallaste.Editor.AudioReviewBatch.Run` (sin `-quit`: el comprobador sale al terminar). Ambos requieren `-projectPath` a este proyecto.

Las pruebas escriben sus resultados fuera de Assets, en `../output/integracion-marco/verification`. La prueba automática comprueba reproducción y parámetros; la calidad perceptiva requiere escucha humana. `DemoSceneBuilder.Deliver` genera capturas, paquete y demostración Windows con gráficos disponibles.

## Revisión de diseño sonoro

Consultar DISENO_SONORO.md para la intención narrativa, paleta por espacio, pruebas de escucha y sincronización propuesta con el responsable de cinemáticas. `SpatialSoundEmitter` evita repetición inmediata, limita el radio y deja pausas entre texturas; `SetSuppressed(bool)` permite retirarlas durante un recuerdo. `SimpleAudioOcclusion` añade filtrado suave de agudos detrás de colliders mediante una máscara configurable; es una aproximación de línea de visión, no propagación acústica física.

`SoundDesignDelivery.Deliver` regenera el laboratorio, valida las muestras mono a 48 kHz, produce una maqueta estéreo de 24 segundos, exporta el paquete y compila Windows. La maqueta es ilustrativa: no graba el resultado 3D del motor. Los efectos incidentales no reemplazan los triggers narrativos ni el controlador de la criatura de otras tareas.
