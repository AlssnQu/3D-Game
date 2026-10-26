# HUD del jugador — Tarea #11

Compatible con Unity 6000.3.23f1, URP y el Input System existentes en el repositorio.

## Recursos

- `Prefabs/PlayerHUD.prefab`: Canvas superpuesto con retícula central y batería en la esquina inferior izquierda.
- `Runtime/FlashlightHud.cs`: actualiza porcentaje, nivel de llenado y color de batería baja; no intercepta clics.
- `Runtime/HudSafeArea.cs`: mantiene el indicador dentro del área segura y responde a cambios de tamaño de pantalla.
- `Scenes/HudReview.unity`: escena independiente para revisar el HUD sin alterar SampleScene.
- `Demo/DemoBatteryReadout.cs`: batería de demostración; no sustituye el sistema de linterna de la tarea #9.

## Probar

Abre `Scenes/HudReview.unity` y pulsa Play. Teclas: 1 = 100%, 2 = 50%, 3 = 15%, 4 = 0%; F = encender o apagar la luz; R = activar descarga gradual; H = ocultar o mostrar HUD.

El HUD de producción no muestra instrucciones de prueba: estos textos solo se añaden en la escena HudReview. La retícula permanece en el centro de la vista, mientras el indicador de batería usa el área segura. CanvasScaler utiliza una referencia de 1920×1080.

## Conectar al sistema de linterna del equipo

Arrastra `PlayerHUD.prefab` a la escena real. Usa una de estas opciones:

1. **Eventos del Inspector:** conecta el evento de batería de la linterna a `FlashlightHud.SetCharge01(float)` si entrega valores de 0 a 1, o a `SetChargePercent(float)` si entrega de 0 a 100.
2. **Interfaz opcional:** el componente de linterna puede implementar `IBatteryReadout`, exponer `Charge01` y asignarse como Battery Source en el HUD o mediante `BindBattery(componente)`.

Se muestra `BATERÍA —` mientras no exista una lectura válida: el prefab no inventa 100% de carga al no tener conexión. Los valores fuera de rango se limitan y los valores no finitos se manejan como batería vacía.

Para ocultar el HUD durante una cinemática llama `SetVisible(false)`; al regresar, `SetVisible(true)`. La lectura puede seguir actualizándose mientras está oculto.

La implementación no cambia los ajustes de Input System, las escenas de compilación ni SampleScene. El control real de la linterna y su inventario siguen siendo responsabilidad de la tarea #9.

## Revisión

La verificación automatizada incluye porcentajes, límites, ausencia de fuente, conexión y visibilidad. Falta revisar con el equipo el tamaño final en el escenario real y conectar el componente definitivo de la linterna cuando se publique.

Esta tarea solo pasa a In review después de aplicar el commit en la rama asignada y abrir el PR dirigido a AlssnQu.

Con Unity cerrado, se puede repetir la prueba con `-batchmode -nographics -projectPath RUTA -executeMethod LoQueCallaste.Hud.Editor.HudReviewBatch.Run`, sin `-quit`: el comprobador entra en Play y cierra al terminar. Guarda `../output/integracion-marco/verification/hud-runtime.json` con los resultados, incluyendo altura visible, color, lectura del proveedor, desconexión y ocultación. `HudSetup.Verify` realiza la comprobación del editor y admite `-quit`; `HudSetup.Deliver` genera capturas, paquete y demo Windows con gráficos disponibles.
