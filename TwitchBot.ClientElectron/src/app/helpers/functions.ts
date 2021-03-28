import { BitEntity, FollowerEntity, SubEntity } from '../database/entity.alert';
import { BadgeEntity } from '../database/entity.badge';
import { IEmoteIndex } from '../models/emote';
import { SMap } from '../models/smap';

import { ChatUserstate, Userstate } from 'tmi.js';

export function wait(time) {
    return new Promise((res) => setTimeout(() => res(), time));
}

export const waitUntil = async (condition, time) => {
    let interval;
    await new Promise((res) => {
        interval = setInterval(() => {
            if (condition()) res();
        }, time);
    });

    clearInterval(interval);
};

export function replace<T extends Object>(text: string, object: T) {
    if (!object) return text;

    Object.keys(object).forEach((key) => (text = text.replace(`{${key}}`, object[key])));
    return text;
}

export function split(message: string, pieces: number, separator: string = ' '): string[] {
    const parts = message.split(separator).filter((m) => !!m);

    const values = [];
    for (let i = 0; i < pieces; i++) {
        values.push(...parts.splice(0, 1));
    }

    if (parts.length > 0) values[values.length - 1] += separator + parts.join(separator);
    return values;
}

export function nonce(length: number) {
    var text = '';
    var possible = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    for (var i = 0; i < length; i++) {
        text += possible.charAt(Math.floor(Math.random() * possible.length));
    }
    return text;
}

export function isBroadcaster(userstate: Userstate) {
    return (userstate && userstate.badges && !!userstate.badges.broadcaster) || userstate.bot;
}

export function seconds(date: Date) {
    const now = new Date();
    const dif = now.getTime() - new Date(date).getTime();

    const seconds = dif / 1000;
    return Math.abs(seconds);
}

export function timeSince(date: Date) {
    if (!date) return '-';

    const s = seconds(date);
    let interval = s / 31536000;
    if (interval > 1) {
        return Math.floor(interval) + 'y';
    }

    interval = s / 2592000;
    if (interval > 1) {
        return Math.floor(interval) + 'm';
    }

    interval = s / 86400;
    if (interval > 1) {
        return Math.floor(interval) + 'd';
    }

    interval = s / 3600;
    if (interval > 1) {
        return Math.floor(interval) + 'h';
    }

    interval = s / 60;
    if (interval > 1) {
        return Math.floor(interval) + 'm';
    }

    return Math.floor(s) + 's';
}

export function randomNum(min: number, max: number) {
    if (min > max) {
        let n = min;
        min = max;
        max = n;
    }

    return Math.floor(Math.random() * (max - min + 1)) + min;
}

export function deepCopy<T>(obj: T): T {
    if (!obj) return null;
    return JSON.parse(JSON.stringify(obj));
}

export function warzoneDrop(): string {
    const pairs: string[] = ['B8'];
    const letters = 'CDEFGH';

    for (let i = 2; i <= 7; i++) {
        for (let j = 0; j < letters.length; j++) pairs.push(letters[j] + i.toString());
    }

    return pairs[randomNum(0, pairs.length - 1)];
}

export function notNull(value: any) {
    return value !== null && typeof value !== 'undefined';
}

export const botUser: ChatUserstate = {
    username: 'WobblesB0T',
    'display-name': 'WobblesB0T',
    color: '#999',
    mod: true,
    bot: true,
};

export function dateAsString(date: Date = new Date()) {
    return `${date.getFullYear()}-${date.getMonth() + 1}-${date.getDate()}`;
}

export function stringToDate(value: string): { value: Date | string; type: 'string' | 'date' } {
    const parts = (value || '').split('-');
    if (parts.length !== 3) return { value: 'N/A', type: 'string' };

    const date = new Date(parseInt(parts[0], 10), parseInt(parts[1], 10) - 1, parseInt(parts[2], 10));
    const now = new Date();
    const now_year = now.getFullYear();
    const now_month = now.getMonth() + 1;
    const now_day = now.getDate();

    if (date.getFullYear() !== now_year || date.getMonth() + 1 !== now_month || ![now_day, now_day - 1].includes(date.getDate())) {
        return { value: date, type: 'date' };
    }

    return {
        value: date.getDate() === now_day ? 'Today' : 'Yesterday',
        type: 'string',
    };
}

export function followMessage(follow: FollowerEntity) {
    return `${follow.from_name} has just followed`;
}

export function bitMessage(bit: BitEntity) {
    return `${bit.user_name || 'Anonymous'} has cheered ${bit.bits_used} bits`;
}

export function subMessage(sub: SubEntity, short: boolean = false) {
    const plan = sub.sub_plan === 'Prime' ? 'Prime' : 'Tier ' + sub.sub_plan[0];
    switch (sub.context) {
        case 'sub':
        case 'resub':
            const months_text = sub.cumulative_months ? `They've subscribed for ${sub.cumulative_months} months` : '';
            const streak_text = sub.streak_months
                ? short
                    ? ` for ${sub.streak_months} months in a row!`
                    : `, currently on a ${sub.streak_months} month streak!`
                : '';
            return `${sub.display_name} subscribed with ${plan}${short ? '' : months_text ? '.' : '!'} ${short ? '' : months_text}${streak_text}`;

        case 'anonresubgift':
        case 'anonsubgift':
        case 'resubgift':
        case 'subgift':
            return `${sub.display_name || 'Anonymous'} has gifted a ${plan} sub to ${sub.recipient_display_name}`;
    }
}

export function toDate(value: string): Date {
    return new Date(value);
}

export function toNum(value: string): number {
    return parseInt(value, 10);
}

export function toBool(value: string): boolean {
    if (!value) return false;

    value = value.toLowerCase();
    return value === 'yes' || value === '1';
}

export function notNullColumns<T>(entity: T, columns: string[]) {
    return columns.filter((c) => notNull(entity[c]));
}

export function emoteMapToArray(emotes: SMap<string[]>): IEmoteIndex[] {
    return Object.keys(emotes || {}).reduce((acc, curr) => {
        emotes[curr].forEach((emote) => {
            const parts = emote.split('-');
            acc.push({ id: curr, from: parseInt(parts[0]), to: parseInt(parts[1]) });
        }, []);

        return acc;
    }, [] as IEmoteIndex[]);
}

export function emoteMessage(input_message: string, emotes: IEmoteIndex[], cheer?: { night_mode: boolean; badges: SMap<BadgeEntity> }) {
    emotes = emotes || [];

    if (!input_message || (emotes.length === 0 && !cheer)) {
        return input_message;
    }

    const toImg = (src, tooltip) => {
        return `<img src="${src}" data-tippy-content="${tooltip}" />`;
    };

    let message = '';
    const letters = input_message.split('');
    for (let i = 0; i < letters.length; i++) {
        const emote = emotes.find((e) => e.from === i);
        if (!emote) {
            message += letters[i];
            continue;
        }

        const url = `https://static-cdn.jtvnw.net/emoticons/v1/${emote.id}/1.0`;
        message += toImg(url, input_message.substring(emote.from, emote.to + 1));
        i += emote.to - emote.from;
    }

    if (cheer && cheer.badges) {
        const words = message.split(' ');
        message = words.reduce((acc, word) => {
            const cheer_key = `${word.toLowerCase()}_${cheer.night_mode ? 'dark' : 'light'}_animated`;
            const badge = cheer.badges[cheer_key];
            if (badge) acc += toImg(badge.image, word);
            else acc += word;

            return acc + ' ';
        }, '');
    }

    return message;
}
