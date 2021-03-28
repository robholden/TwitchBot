import * as confetti from 'canvas-confetti';
import { random } from 'lodash';

export interface ConfettiOptions {
    particleCount?: number;
    angle?: number;
    spread?: number;
    startVelocity?: number;
    decay?: number;
    gravity?: number;
    ticks?: number;
    origin: { x: number; y: number };
    shapes?: string[];
    zIndex?: number;
    colors?: string[];
}

export class ConfettiCannon {
    confetti: any;

    constructor() {
        this.confetti = confetti.default;
    }

    randomFire() {
        this.fire({
            origin: { x: random(25, 45) / 100, y: 1 },
            angle: random(55, 125),
            spread: random(50, 100),
            particleCount: random(50, 100),
        });
    }

    randomMultiFire(shots: number = 1, duration: number = 500) {
        let i = 0;
        const fire = () => {
            if (i >= shots) return;

            this.randomFire();
            i++;

            setTimeout(() => fire(), duration);
        };
        fire();
    }

    fire(options: ConfettiOptions) {
        this.confetti(options);
    }
}
