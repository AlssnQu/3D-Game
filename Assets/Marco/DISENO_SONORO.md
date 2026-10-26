# LO QUE CALLASTE — Diseño sonoro de Marco

## Intención

Que el jugador perciba un colegio reconocible antes de sentirlo amenazante. El miedo nace de un sonido fuera de campo, de su procedencia y de las pausas que le siguen. Los recuerdos de acoso y la culpa del testigo guían la experiencia. Evitar convertir el colegio en un campo de batalla o cubrirlo con gritos continuos.

## Paleta implementada en el laboratorio

| Lugar / acción | Sonido y tratamiento | Función |
|---|---|---|
| Caminar | 4 pasos de concreto, sin repetición inmediata, pitch 0.97–1.03 | Presencia corporal y contraste con otros movimientos |
| Pasillo | Ambiente propio tenue y metal de Kenney localizado en un casillero | Inducir una mirada hacia el origen sin confirmar una criatura |
| Aula | 3 crujidos de madera de laleksic, pitch 0.94–1.04 | Dar vida al espacio vacío, conservando incertidumbre |
| Baño | Bisagras y agua de laleksic, reverberación Bathroom | Identidad acústica húmeda y cerrada |
| Pared interpuesta | Filtrado progresivo de agudos de los emisores puntuales | Reducir la sensación de sonidos que atraviesan paredes sin cambio |
| Pausas | Intervalos de 9–25 s según emisor; separación global de 2 s | Dejar espacio al jugador y evitar acumulación de impactos |

Son parámetros iniciales de revisión. No representan una mezcla final aprobada. Solo suenan las texturas cercanas al jugador; se detienen al salir de su radio. Los ambientes siguen siendo discretos bucles propios. Cada textura dispone de una fuente independiente: no comparte canal con diálogos ni con pasos.

## Integración técnica

`SpatialSoundEmitter` añade variación y pausas a las texturas ambientales; no implementa los triggers narrativos de #18. `SimpleAudioOcclusion` usa una comprobación de línea de visión cada 0.2 s y un AudioLowPassFilter. Su máscara Obstacles permite seleccionar los colliders que deben amortiguar el sonido. Ignora triggers y colliders del oyente. Es una aproximación: no calcula materiales, reflexiones, difracción ni puertas abiertas por geometría acústica.

Los presets Hallway y Bathroom continúan usando zonas esféricas. Ajustar radios y superposición al mapa definitivo. La oclusión añadida no convierte esas zonas en simulación física.

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

## Revisión con el jugador

1. Empezar a un volumen cómodo. Comprobar izquierda/derecha, aproximación y alejamiento con auriculares; repetir en altavoces y mono.
2. Estar quieto al menos 30 s en pasillo, aula y baño. Las texturas deben dejar pausas y proceder de un lugar identificable.
3. Colocar una pared entre emisor y jugador y retirarla: el cambio de agudos debe ser gradual, sin alterar los diálogos.
4. Revisar el mapa definitivo en puertas y esquinas; ajustar máscaras, radios, distancias y ganancia. Las pruebas automáticas no sustituyen esta escucha.
5. Durante el flashback definitivo, comprobar prioridad de voz, subtítulos, suspensión de texturas y restauración al finalizar o cancelar. Esta parte queda pendiente de la integración del equipo.

## Herramientas

La entrega usa audio nativo de Unity, AudioReverbZone, AudioLowPassFilter y el Timeline que el proyecto ya incluye. No necesita instalar un middleware para funcionar. Si se edita la paleta en una herramienta externa, conservar los originales y registrar los cambios en SOURCES.json. No aplicar reverberación permanente a los efectos secos que ya reciben reverberación en el motor.
