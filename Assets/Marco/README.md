# Audio espacial — Marco · Tarea #17

Base: Unity 6000.3.23f1, URP 17.3 e Input System 1.20 del equipo. La escena original `Assets/Scenes/SampleScene.unity` y la configuración del proyecto se conservan.

## Integración sobre el colegio actualizado

Base verificada en main 26e6c06, con el movimiento, linterna y raycast del equipo. Abrir `Assets/Marco/Scenes/SchoolAudioPreview.unity` y pulsar Play. WASD mueve, el ratón mira, Ctrl agacha, Espacio salta, F acciona la linterna y R reproduce o interrumpe el recuerdo de revisión.

La escena parte de `Assets/Scenes/Example.unity` e incluye su jugador. Los ecos cubren seis secciones del pasillo y el baño. Casilleros, puertas y murmullos tienen emisores localizados con pausas variables y filtrado por pared. Los pasos distinguen madera y baldosa, y los clics siguen el estado de la linterna. La batería del equipo alimenta el HUD mediante `TeamBatteryReadout`.

`MemoryAudioLink` conecta PlayableDirector con la mezcla, el fundido y la visibilidad del HUD. Conserva el estado de cada control y lo restaura al terminar, cancelar o desactivar el enlace. Si Timeline se reinicia durante el fundido de salida, mantiene el bloqueo y la captura original de estados hasta salir de la nueva reproducción.

La secuencia de cinco segundos permite revisar transición y sonido. Quedan pendientes las voces autorizadas, el teléfono, el disparador narrativo y la aprobación de la mezcla mediante escucha. La reverberación usa zonas esféricas y el filtrado aproxima la línea de visión.

### Reproducir la validación

Con Unity cerrado, ejecutar `-batchmode -nographics -projectPath RUTA -executeMethod LoQueCallaste.Editor.SchoolPreviewCheck.Run`, sin `-quit`. Genera la escena desde Example, entra en Play y termina con código 0 si pasan las comprobaciones. El registro de esta revisión contiene 71 comprobaciones, incluidas 20 repeticiones con una paleta incompleta. El conjunto anterior de 69 comprobaciones de audio también pasó.

`Marco > Audio > Compilar prueba del colegio para Windows` genera `../output/integracion-marco/Colegio-Windows/Unspoken-Audio.exe`. La compilación y el arranque se verificaron en Windows con Unity 6000.3.23f1. Durante las ejecuciones del editor aparece una excepción de `UnityEditor.Search.SearchDatabase` al indexar el proyecto. Las comprobaciones de Play terminan correctamente.

## Laboratorio de componentes

`AudioCinematicsLab.unity` conserva la geometría provisional para comprobar los componentes de forma aislada.

## Tarea #17: audio espacial y reverberación

Escena de comprobación: `Assets/Marco/Scenes/AudioCinematicsLab.unity`.

- `PositionalAudio`: configura AudioSource 3D, atenuación, distancia, Doppler desactivado y envío a reverb.
- `SpatialAudioProfile`: perfiles editables para efectos y pasos; admite un AudioMixerGroup del proyecto.
- `ReverbPasillo.prefab` y `ReverbBano.prefab`: presets y radios editables. Las zonas son esféricas; ajustar su alcance al mapa real.
- `AudioPlayer`: identificador en la raíz del jugador; no agrega un segundo AudioListener.
- `DemoPlayerController`: recorrido de prueba compatible con el Input System del equipo. No reemplaza el controlador definitivo.

Para generar el laboratorio: menú Marco → Crear escena de audio y cinemáticas. Este comando pide guardar la escena abierta y genera únicamente recursos en Assets/Marco; no cambia las escenas de compilación ni el sistema de controles del equipo.

Controles: WASD, ratón, F linterna, Esc liberar cursor y clic para volver; M abre ajustes de sonido. Los ambientes propios se combinan con pasos de baldosa y madera, metal, crujidos, bisagras, agua, clics y texturas de susurro de una paleta documentada de 24 archivos en Audio/Curated/CREDITS.md. La mezcla requiere revisión auditiva en el mapa final.

La geometría de AudioCinematicsLab es provisional. SchoolAudioPreview utiliza los recursos del colegio incluidos por el equipo.

## Flujo de entrega

El trabajo se prepara localmente sin publicar ramas ni modificar main. El líder crea la rama de cada tarea; después se sincroniza main, se traen las ramas asignadas, se aplican los commits correspondientes, se verifica en Unity y se abre el PR dirigido a AlssnQu. Solo entonces se mueve a In review.

## Verificación reproducible

En Unity cerrado, ejecutar el editor en batch con `-executeMethod LoQueCallaste.Editor.SpatialAudioSetup.VerifySpatialScene -quit` para revisar la configuración. Para comprobar reproducción y reverberación en Play: `-batchmode -nographics -executeMethod LoQueCallaste.Editor.AudioReviewBatch.Run` (sin `-quit`: el comprobador sale al terminar). Ambos requieren `-projectPath` a este proyecto.

Las pruebas escriben sus resultados fuera de Assets, en `../output/integracion-marco/verification`. La prueba automática comprueba reproducción y parámetros; la calidad perceptiva requiere escucha humana. `DemoSceneBuilder.Deliver` genera capturas, paquete y demostración Windows con gráficos disponibles.

## Revisión de diseño sonoro

Consultar DISENO_SONORO.md para la intención narrativa, paleta por espacio, pruebas de escucha y sincronización propuesta con el responsable de cinemáticas. `SpatialSoundEmitter` evita repetición inmediata, limita el radio y deja pausas entre texturas; `SetSuppressed(bool)` permite retirarlas durante un recuerdo. `SimpleAudioOcclusion` añade filtrado suave de agudos detrás de colliders mediante una máscara configurable; es una aproximación de línea de visión, no propagación acústica física.

`SoundDesignDelivery.Deliver` regenera el laboratorio, valida las muestras mono a 48 kHz, produce una maqueta estéreo de 36 segundos, exporta el paquete y compila Windows. La maqueta es ilustrativa: no graba el resultado 3D del motor. Los efectos incidentales no reemplazan los triggers narrativos ni el controlador de la criatura de otras tareas.

SchoolAudioDirector ofrece volumen general/ambiente/efectos/voces, impactos suaves y mezcla de recuerdo mediante BeginCinematic/EndCinematic. SurfaceFootsteps expone pasos por superficie y FlashlightSwitchAudio confirma cambios de linterna sin sustituir su lógica. Los puntos de enlace y los límites de la mezcla están en DISENO_SONORO.md.

La selección de texturas compara la grabación anterior por referencia: evita repetirla aunque aparezca duplicada en la paleta y omite entradas sin asignar. Si solo queda una grabación válida, permite reutilizarla como alternativa controlada.

## Paleta original y modo de escucha

`Audio/Original` contiene dos respiraciones, un roce de tela y una señal de llamada sin respuesta, sintetizados para Unspoken. Los WAV son mono PCM de 16 bits a 48 kHz. SOURCES.json conserva duración, picos, RMS y hashes. Se reproducen con `python Tools/Audio/generate_original_palette.py`, usando la biblioteca estándar de Python. Los importadores conservan sus picos originales entre -12 y -16 dBFS.

En SchoolAudioPreview, M muestra el panel de escucha. Con el panel visible, 1 alterna ambiente, 2 efectos, 3 recuerdos, 4 impactos suaves, 5 restaura la mezcla inicial y - / = ajustan el volumen general. B prueba respiración, V tela y N la señal telefónica. Las pruebas de criatura colocan la fuente a tres metros a la derecha y un metro por delante del oyente. El jugador sigue recorriendo el colegio mientras compara la mezcla. Durante un recuerdo se oculta el panel y se suspenden las audiciones.

`Prefabs/CreatureAudio.prefab` contiene dos emisores locales centrados y configurados como fuentes mono 3D. Para integrarlo, colocar el prefab como hijo del modelo de la criatura y llamar a `SpatialSoundEmitter.TryPlay()` sobre Respiración o Roce de tela desde el script de IA. `automatic` está desactivado. Se conserva el enfriamiento compartido de dos segundos, el radio audible y la supresión durante recuerdos. Al registrar estos emisores en otro escenario, añadirlos a `SchoolAudioDirector.textures` y sus fuentes a `channels`, con categoría Effects. La escena de revisión ya tiene esa conexión.

La revisión actual aprobó 71 comprobaciones, incluidas las muestras después de importarlas en Unity y el prefab sin referencias a objetos de la escena. El tono telefónico queda disponible para la llamada narrativa. El guion, las voces autorizadas y los eventos de IA requieren la integración de sus responsables. El balance perceptivo se aprueba escuchando el juego en auriculares y altavoces.

## Muestra de la mezcla del motor

`Review/Marco/SchoolAudioPreview.wav` permite escuchar la salida estéreo de Unity con posiciones de oyente guiadas. Se capturan pasos, ambiente, murmullo, sonidos de criatura, señal telefónica y la transición de revisión. El informe SchoolAudioPreview.json registra la versión del motor, duración, frecuencia de muestreo, pico y RMS.

La grabación usa [AudioRenderer.Render](https://docs.unity.cn/6000.3/Documentation/ScriptReference/AudioRenderer.Render.html). Para regenerarla con Unity cerrado, ejecutar `-batchmode -nographics -projectPath RUTA -executeMethod LoQueCallaste.Editor.SchoolAudioCaptureBatch.Run`. El proceso termina al completar la muestra. La captura recorre posiciones de escucha preparadas para revisar la acústica del escenario. Los disparadores y los desplazamientos finales de IA y teléfono requieren sus componentes del equipo.

La medición motivó un ajuste de cobertura ambiental. El pasillo mantiene su volumen estable hasta cuatro metros del emisor y el baño hasta 1,5 metros, con atenuación posterior. Los pasos se ajustaron a 0,55 de volumen de fuente. La prueba automática comprueba los nuevos radios y el render rechaza muestras vacías, valores no finitos, saturación y señales de revisión omitidas.
