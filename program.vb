REM Константы
LET R = 6371032;
LET G = 6.6742e-11;
LET pi = 3.14159265;
LET M_e = 5.9726e24;
LET GMP = 216;
LET target_angle = 81;
REM Параметры аппарата
LET m0 = ;
LET M = 0.000001;
LET Mmax = 0.0026;
LET a = 0.1503;
LET Iz = 1 / 6 * a * a * m0;
LET eps = Mmax / Iz;
LET heater_on = FALSE;
REM Полетные параметры
LET horb = 619000;
LET vorb = SQRT(G * M_e / (R + horb));

LET w = -0.061899;
LET M0 = -4.324165e-05;
LET t = 508.52283;

REM Углы начала/конца передачи
LET tr_start_angle = GMP - ACOS(R / (R + horb));
LET tr_stop_angle = GMP + ACOS(R / (R + horb));

LET dw = 0.01;
LET moment = FALSE;
LET da = 0.0001;
LET sent = FALSE;

WHEN cpu.flight_time == 0.0 DO
    CALL cpu.set_cycle(1);
    CALL orientation.start_torsion(M0);
END;

WHEN cpu.cycle == 1 AND cpu.flight_time >= t DO
    CALL cpu.set_cycle(2);
    CALL orientation.stop_torsion();
END;

WHEN cpu.cycle == 2 DO
    IF moment == TRUE AND ABS(orientation.angular_velocity - w) < dw THEN
        CALL orientation.stop_torsion();
        moment = FALSE;
		CALL cpu.set_cycle(3);
    ELSE
        IF orientation.angular_velocity > w THEN
            CALL orientation.start_torsion(0 - M);
            moment = TRUE;
        END;
        IF orientation.angular_velocity < w THEN
            CALL orientation.start_torsion(M);
            moment = TRUE;
        END;
    END;
END;

WHEN cpu.cycle == 3 AND navigation.angle + 1 > target_angle AND DO 
	IF moment == TRUE AND ABS(orientation.angular_velocity - w) < dw THEN
        CALL orientation.stop_torsion();
        moment = FALSE;
    ELSE
        IF orientation.angular_velocity > w THEN
            CALL orientation.start_torsion(0 - M);
            moment = TRUE;
        END;
        IF orientation.angular_velocity < w THEN
            CALL orientation.start_torsion(M);
            moment = TRUE;
        END;
    END;
	CALL load.set_mode("ON");
	CALL cpu.set_cycle(4);
END;

WHEN cpu.cycle == 4 AND navigation.angle - 1 > target_angle DO
	IF moment == TRUE AND ABS(orientation.angular_velocity - w) < dw THEN
        CALL orientation.stop_torsion();
        moment = FALSE;
    ELSE
        IF orientation.angular_velocity > w THEN
            CALL orientation.start_torsion(0 - M);
            moment = TRUE;
        END;
        IF orientation.angular_velocity < w THEN
            CALL orientation.start_torsion(M);
            moment = TRUE;
        END;
    END;
	CALL radio.set_mode("ON");
	CALL load.set_mode("OFF");
	CALL radio.set_mode("OFF");
	CALL cpu.set_cycle(5);
END;

WHEN cpu.cycle == 5 DO
	IF moment == TRUE AND ABS(orientation.angular_velocity - w) < dw THEN
        CALL orientation.stop_torsion();
        moment = FALSE;
    ELSE
        IF orientation.angular_velocity > w THEN
            CALL orientation.start_torsion(0 - M);
            moment = TRUE;
        END;
        IF orientation.angular_velocity < w THEN
            CALL orientation.start_torsion(M);
            moment = TRUE;
        END;
    END;
	IF ABS(navigation.angle - tr_start_angle) < da THEN
		CALL radio.set_mode("ON");
	END;
	IF ABS(navigation.angle - tr_stop_angle) < da THEN
		CALL radio.set_mode("OFF");
	END;
END;


WHEN navigation.dark_side == TRUE AND heat_control.temperature < 283 DO
	heat_control.start_heating();
	heater_on = TRUE;
END;

WHEN navigation.dark_side == FALSE DO
	heat_control.stop_heating();
	heater_on = FALSE;
END;

WHEN heat_control.temperature > 310 AND heater_on == TRUE DO
	heat_control.stop_heating();
	heater_on = TRUE;
END;

	