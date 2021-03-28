#!/usr/bin/env node

const yargsI = require('yargs-interactive');
const axios = require('axios');
const signalR = require('@microsoft/signalr');
const colors = require('colors');

console.log('------------------------------------------------');
console.log('');
console.log('            Twitch Bot');
console.log('');
console.log('------------------------------------------------');

async function start() {
    const client_values = await yargsI()
        .usage('$0 <command> [args]')
        .interactive({
            interactive: { default: true },
            host: {
                type: 'list',
                choices: ['http://localhost:8999'],
                default: 'http://localhost:8999',
                describe: 'Select the server address',
                prompt: 'always'
            },
            username: { type: 'input', default: 'Rob_0', describe: 'Enter your username', prompt: 'always' },
            password: { type: 'input', default: 'dev', describe: 'Enter your password', prompt: 'always' }
        });

    console.log(`Connecting to: ${client_values.host}@${client_values.username}:${client_values.password}`);

    // Get access token
    const res = await axios.post(`${client_values.host}/auth/login`, { username: client_values.username, password: client_values.password });
    const data = res.data;

    console.log(`Authentication successful => ${data.token}`.green);
    console.log();

    const hub_url = `${client_values.host}/hubs/event`;
    console.log(`Establishing socket connection to: ${hub_url}`);

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(hub_url, { accessTokenFactory: () => data.token })
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.None)
        .build();

    await connection.start();

    console.log('Connection established with hub'.green);
    console.log();

    console.log(`${data.subscriptions.length} subscriptions found`);
    for (let i in data.subscriptions) {
        const sub = data.subscriptions[i];
        connection.on(sub.type, (evt) => parseSubEvent(sub.type, evt));
        console.log(`Listening to ${sub.type} => ${client_values.host}/#/${data.token}/${client_values.username.toLowerCase()}/${sub.method}`.grey);
    }
    console.log();
}

function parseSubEvent(type, evt) {
    switch (type) {
        case 'channel.channel_points_custom_reward_redemption.add':
            console.log(`${evt.user_name} just redeemed ${evt.reward.title} (${evt.reward.cost}pts)`);
            break;

        case 'channel.follow':
            console.log(`${evt.user_name} just followed`);
            break;
    }
}

start()
    .then(async () => {})
    .catch((err) => console.log(err));
