(* SCADA self-study: washing machine practice program for OpenPLC *)
(* No real sensors: water level and temperature are simulated *)
(* Address rules: %IX0.0=10001 %QX0.0=00001 %IW0=30001 %QW0=40001 *)
(* Monitoring page needs: one variable per line, single spaces, no comment on the same line *)
PROGRAM washer
  VAR
    (* 10001 start button, 10003 door closed: not wired, always 0 *)
    StartBtn AT %IX0.0 : BOOL;
    DoorClosed AT %IX0.2 : BOOL;
    (* 00001 inlet valve, 00005 green lamp, 00007 red lamp *)
    InletValve AT %QX0.0 : BOOL;
    GreenLamp AT %QX0.4 : BOOL;
    RedLamp AT %QX0.6 : BOOL;
    (* 40001 drum speed 1152=50rpm, 40002 heartbeat, 40003 level 0..27648=0..50cm, 40004 dryer temp *)
    DrumSpeed AT %QW0 : INT;
    Heartbeat AT %QW1 : INT;
    WaterLevel AT %QW2 : INT;
    DryTemp AT %QW3 : INT;
  END_VAR
  VAR
    Tick : TON;
  END_VAR

  (* pulse once per second *)
  Tick(IN := NOT Tick.Q, PT := T#1000ms);

  IF Tick.Q THEN
    Heartbeat := Heartbeat + 1;
    IF Heartbeat >= 30000 THEN
      Heartbeat := 0;
    END_IF;
    (* simulated filling: +1382 per second, about 2.5 cm *)
    WaterLevel := WaterLevel + 1382;
    IF WaterLevel > 27648 THEN
      WaterLevel := 0;
    END_IF;
  END_IF;

  DrumSpeed := 1152;
  DryTemp := 13824;
  InletValve := WaterLevel < 13824;
  GreenLamp := TRUE;
  RedLamp := DryTemp > 16589;
END_PROGRAM

CONFIGURATION Config0
  RESOURCE Res0 ON PLC
    TASK task0(INTERVAL := T#20ms, PRIORITY := 0);
    PROGRAM instance0 WITH task0 : washer;
  END_RESOURCE
END_CONFIGURATION
