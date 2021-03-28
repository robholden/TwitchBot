const { app, BrowserWindow, ipcMain, nativeTheme } = require('electron');
const fs = require('fs');
const url = require('url');
const path = require('path');
const isDev = require('electron-is-dev');
const data_path = isDev ? '' : app.getPath('userData');
const windowStateKeeper = require('electron-window-state');

let win;

function initWindow() {
    let mainWindowState = windowStateKeeper({
        defaultWidth: 1300,
        defaultHeight: 700,
    });

    win = new BrowserWindow({
        x: mainWindowState.x,
        y: mainWindowState.y,
        width: mainWindowState.width,
        height: mainWindowState.height,
        webPreferences: {
            nodeIntegration: true,
            webviewTag: true,
            enableRemoteModule: true,
            scrollBounce: true,
            webSecurity: false,
        },
        icon: path.join(__dirname, '/ng-dist/assets/logo.png'),
    });

    // Let us register listeners on the window, so we can update the state
    // automatically (the listeners will be removed when the window is closed)
    // and restore the maximized or full screen state
    mainWindowState.manage(win);

    // Electron Build Path
    win.loadURL(
        url.format({
            pathname: path.join(__dirname, '/ng-dist/index.html'),
            protocol: 'file:',
            slashes: true,
        })
    );

    ipcMain.handle('dark-mode:toggle', () => {
        if (nativeTheme.shouldUseDarkColors) {
            nativeTheme.themeSource = 'light';
        } else {
            nativeTheme.themeSource = 'dark';
        }
        return nativeTheme.shouldUseDarkColors;
    });

    ipcMain.handle('dark-mode:system', () => {
        nativeTheme.themeSource = 'system';
    });

    // Initialize the DevTools.
    // appWindow.webContents.openDevTools();
    // appWindow.removeMenu();

    win.on('closed', function () {
        win = null;
    });
}

app.on('ready', initWindow);

// Close when all windows are closed.
app.on('window-all-closed', function () {
    // On macOS specific close process
    if (process.platform !== 'darwin') {
        app.quit();
    }
});

app.on('activate', function () {
    if (win === null) {
        initWindow();
    }
});

var cfg_path = path.join(data_path, 'config.json');

getCfg = () => {
    return JSON.parse(fs.readFileSync(cfg_path, 'utf8'));
};

exports.getValue = (key) => {
    var cfg = getCfg();
    if (key in cfg) return cfg[key];

    return '';
};

exports.setValue = (key, value) => {
    var cfg = getCfg();
    cfg[key] = value;

    fs.writeFileSync(cfg_path, JSON.stringify(cfg));
};
