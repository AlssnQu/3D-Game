# LO QUE CALLASTE — Diseño sonoro de Marco

## Intención

Que el jugador perciba un colegio reconocible antes de sentirlo amenazante. El miedo nace de un sonido fuera de campo, de su procedencia y de las pausas que le siguen. Los recuerdos de acoso y la culpa del testigo guían la experiencia. Evitar convertir el colegio en un campo de batalla o cubrirlo con gritos continuos.

## Paleta implementada en el laboratorio

| Lugar / acción | Sonido y tratamiento | Función |
|---|---|---|
| Caminar | 4 pasos de baldosa/concreto y 4 de madera, según superficie y distancia recorrida; pitch 0.97–1.03 | Presencia corporal; sin pasos al estar quieto o en el aire |
| Pasillo | Ambiente propio tenue y metal de Kenney localizado en un casillero | Inducir una mirada hacia el origen sin confirmar una criatura |
| Aula | 3 crujidos de madera de laleksic, pitch 0.94–1.04 | Dar vida al espacio vacío, conservando incertidumbre |
| Baño | Bisagras y agua de laleksic, reverberación Bathroom | Identidad acústica húmeda y cerrada |
| Pared interpuesta | Filtrado progresivo de agudos de los emisores puntuales | Reducir la sensación de sonidos que atraviesan paredes sin cambio |
| Pausas | Intervalos de 9–25 s según emisor; separación global de 2 s | Dejar espacio al jugador y evitar acumulación de impactos |
| Murmullo de aula | 2 texturas transformadas de un susurro CC0, intervalo 22–34 s y volumen bajo | Presencia humana ambigua fuera de campo, sin escribir diálogo nuevo |
| Linterna | 2 clics grabados de interruptor, fuente separada de los pasos | Confirmación discreta del cambio de estado |
| Recuerdo | 2 motivos de entrada y salida, con crujido transformado, metal y resonancia propia | Identidad sonora común para las transiciones; clips listos para el Timeline del equipo |

Son parámetros iniciales de revisión. No representan una mezcla final aprobada. Solo suenan las texturas cercanas al jugador; se detienen al salir de su radio. Los ambientes siguen siendo discretos bucles propios. Cada textura dispone de una fuente independiente: no comparte canal con diálogos ni con pasos.

## Integración técnica

`SpatialSoundEmitter` añade variación y pausas a las texturas ambientales; no implementa los triggers narrativos de #18. `SimpleAudioOcclusion` usa una comprobación de línea de visión cada 0.2 s y un AudioLowPassFilter. Su máscara Obstacles permite seleccionar los colliders que deben amortiguar el sonido. Ignora triggers y colliders del oyente. Es una aproximación: no calcula materiales, reflexiones, difracción ni puertas abiertas por geometría acústica.

Los presets Hallway y Bathroom continúan usando zonas esféricas. Ajustar radios y superposición al mapa definitivo. La oclusión añadida no convierte esas zonas en simulación física.

`SurfaceFootsteps.Advance(metros, grounded, superficie)` recibe desplazamiento real del controlador; también dispone de `PlayStep(superficie)` para eventos de animación. `FootstepSurface` marca madera en el suelo del aula; sin marca se usa baldosa. `FlashlightSwitchAudio.OnStateChanged(bool)` se conecta al evento de la linterna definitiva; no controla carga ni encendido. El laboratorio conecta ambas piezas a su controlador de prueba.

## Mezcla por categorías y comodidad

`SchoolAudioDirector` controla ganancias locales de fuentes registradas en categorías Ambience, Effects y Dialogue. No es un AudioMixer nativo ni un compresor: es una capa ligera de control de volumen que puede coexistir con grupos de AudioMixer siempre que el equipo defina quién controla cada ganancia. Una fuente debe pertenecer a un único director. El script no cambia el volumen global de AudioListener.

- General: 85% al iniciar la escena de prueba; ambiente, efectos y voces son ajustables por separado mediante métodos SetMaster, SetAmbience, SetEffects y SetDialogue.
- Impactos suaves: multiplica los efectos por 0.6. Mantiene la posición y la categoría de las voces.
- Recuerdo: BeginCinematic reduce el ambiente al 18% y los efectos al 45% de su nivel de mezcla durante un fundido inicial de 0.8 s. Suprime las texturas ambientales y reproduce el motivo de entrada.
- Fin/cancelación: EndCinematic recupera niveles gradualmente, conserva las texturas que ya estaban suprimidas y reproduce el motivo de salida. Llamar también desde la cancelación de la secuencia. Desactivar el director restaura las ganancias de autoría de sus fuentes.

Las voces de personajes no están grabadas ni asignadas en esta entrega. La categoría Dialogue se deja preparada para su integración y se valida mediante una fuente de prueba. El control de voces del panel no altera los murmullos ambientales, que pertenecen a Effects.

En la demo, M abre los ajustes y suspende únicamente el movimiento del controlador de prueba; M o Esc cierran el panel y restauran su estado. El panel pertenece al laboratorio: el menú definitivo del juego podrá llamar a la misma API. Los valores son de sesión; no se escriben preferencias globales ni ajustes compartidos.

La distancia máxima de AudioSource en modo logarítmico no garantiza silencio fuera del radio. Por eso el emisor comprueba por separado `audibleRadius`. Los efectos se mantienen mono para su posición 3D. No añadir un segundo AudioListener al importar los prefabs en el controlador del equipo.

## Acuerdo de integración con cinemáticas

La tarea #15 está asignada a otra integrante en el tablero consultado. Lo siguiente es una propuesta de coordinación para el responsable de la secuencia; no implica que el flashback definitivo esté integrado.

| Momento narrativo | Audio | Enlace previsto |
|---|---|---|
| Exploración | Ambiente y texturas localizadas | Emisores activos; HUD visible |
| Inicio del recuerdo | Detener texturas incidentales; bajar ambiente con un fundido | `SpatialSoundEmitter.SetSuppressed(true)` en cada emisor; `FlashlightHud.SetVisible(false)` |
| Objeto y transición | Foley del objeto sincronizado con el contacto | Pista Audio de Timeline con clip autorizado específico del objeto |
| Voz del recuerdo | Prioridad e inteligibilidad; subtítulo sincronizado con el diálogo | AudioTrack + subtítulos del sistema del equipo; no asignar voces provisionales como finales |
| Pausa significativa | Dejar respirar la frase, sin golpe aleatorio añadido | Mantener emisores suprimidos |
| Fin o cancelación | Recuperar niveles y texturas con suavidad | Restaurar el volumen previo; `SetSuppressed(false)`; recuperar la visibilidad previa del HUD |

Los eventos de finalización, cancelación y salida de escena deben restaurar los estados anteriores. El controlador de la cinemática debe guardar cuáles emisores estaban ya suprimidos y si el HUD estaba visible, en vez de habilitar todo indiscriminadamente. El prototipo anterior de flashback es una referencia independiente, no código listo para copiar sin adaptar al controlador actual.

Para usar el director, conectar inicio a BeginCinematic y fin/cancelación a EndCinematic; el director guarda y restaura la supresión de sus texturas. Es el dueño único de esa mezcla: no aplicar simultáneamente un segundo fundido manual a esas mismas fuentes. El HUD conserva su enlace independiente y su estado previo. Si Timeline reproduce los motivos por AudioTrack, dejar memoryIn/memoryOut sin asignar en el director para evitar duplicarlos.

## Revisión con el jugador

1. Empezar a un volumen cómodo. Comprobar izquierda/derecha, aproximación y alejamiento con auriculares; repetir en altavoces y mono.
2. Estar quieto al menos 30 s en pasillo, aula y baño. Las texturas deben dejar pausas y proceder de un lugar identificable.
3. Colocar una pared entre emisor y jugador y retirarla: el cambio de agudos debe ser gradual, sin alterar los diálogos.
4. Revisar el mapa definitivo en puertas y esquinas; ajustar máscaras, radios, distancias y ganancia. Las pruebas automáticas no sustituyen esta escucha.
5. Durante el flashback definitivo, comprobar prioridad de voz, subtítulos, suspensión de texturas y restauración al finalizar o cancelar. Esta parte queda pendiente de la integración del equipo.

## Herramientas

La entrega usa audio nativo de Unity, AudioReverbZone, AudioLowPassFilter y el Timeline que el proyecto ya incluye. No necesita instalar un middleware para funcionar. Si se edita la paleta en una herramienta externa, conservar los originales y registrar los cambios en SOURCES.json. No aplicar reverberación permanente a los efectos secos que ya reciben reverberación en el motor.


## Recursos originales para criatura y llamada

La respiración combina ruido filtrado, una envolvente de aire y una modulación grave leve. Las dos variantes permiten alternar tomas. El roce de tela usa pulsos irregulares para acompañar un desplazamiento de la criatura. La señal telefónica incorpora pulsos tonales con ataques y salidas suaves. Los cuatro recursos se generan mediante Tools/Audio/generate_original_palette.py y sus medidas están en Audio/Original/SOURCES.json.

El prefab CreatureAudio separa respiración y roce para que la programadora pueda dispararlos según el estado del personaje. El radio se mantiene localizado y las fuentes respetan la categoría de efectos y la reducción de impactos. En el recuerdo se detienen junto con las texturas del entorno. La señal telefónica se trata como sonido subjetivo y entra en la categoría de recuerdos.

Para la revisión por escucha, comparar primero el ambiente al recorrer pasillo y baño, después añadir efectos y por último probar R. Usar M para aislar categorías y comprobar que la respiración sea reconocible sin tapar pasos o diálogo. B y V permiten comparar la posición de prueba antes de conectar el modelo. Registrar el balance aprobado con el equipo antes de considerarlo mezcla final.


## Recorrido de revisión renderizado en Unity

El archivo Review/Marco/SchoolAudioPreview.wav presenta esta secuencia de escucha.

| Tiempo aproximado | Contenido |
| --- | --- |
| 0 a 8 s | Ambiente del pasillo y pasos |
| 8 a 11 s | Murmullo localizado |
| 11 a 15 s | Respiración de criatura a la derecha |
| 15 a 17 s | Roce de tela |
| 17 a 22 s | Desplazamiento del oyente hacia el baño |
| 22 a 26 s | Señal telefónica subjetiva |
| 26 a 31 s | Transición al recuerdo y mezcla reducida |
| 31 a 36 s | Recuperación del ambiente |

Se conserva el nivel de salida del motor en la grabación. El informe JSON ofrece pico y RMS para comparar futuras revisiones. La evaluación auditiva del equipo determina el balance final en auriculares, altavoces y reproducción mono.
