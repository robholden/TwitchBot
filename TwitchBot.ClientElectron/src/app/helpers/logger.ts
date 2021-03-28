const { createLogger, format, transports } = (<any>window).require('winston');
const log = createLogger({
    level: 'debug',
    format: format.combine(
        format.timestamp({
            format: 'YYYY-MM-DD HH:mm:ss',
        }),
        format.errors({ stack: true }),
        format.splat(),
        format.json()
    ),
    defaultMeta: { service: 'wobbles-b0t' },
    transports: [new transports.File({ filename: 'logs/error.log', level: 'error' }), new transports.File({ filename: 'logs/combined.log' })],
});

export class Logger {
    static debug(...args: any) {
        log.debug(...args);
        console.log(...args);
    }

    static warn(...args: any) {
        log.warn(...args);
        console.warn(...args);
    }

    static error(...args: any) {
        log.error(...args);
        console.error(...args);
    }
}
