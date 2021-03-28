export function wait(time: number) {
    return new Promise<void>((res) => setTimeout(() => res(), time));
}

export const waitUntil = async (condition: () => boolean, time: number) => {
    let interval;
    await new Promise<void>((res) => {
        interval = setInterval(() => {
            if (condition()) res();
        }, time);
    });

    clearInterval(interval);
};
