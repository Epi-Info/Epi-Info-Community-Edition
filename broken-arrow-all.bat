

::taskkill /f /im explorer.exe
::taskkill /f /im chrome.exe
::taskkill /f /im electron.exe
::taskkill /f /im chrome.exe

::npm cache clean --force

::pause

::taskkill /f /im cmd.exe
::start explorer.exe

:: ====================================================
:: if yarn dev doesn't open app
:: ====================================================
:: C:\_CODE\shed\epi-info-maps>yarn dev
:: yarn run v1.12.3
:: $ cross-env START_HOT=1 node -r @babel/register ./internals/scripts/CheckPortInUse.js && cross-env START_HOT=1 yarn start-renderer-dev
:: $ cross-env NODE_ENV=development webpack-dev-server --config configs/webpack.config.renderer.dev.babel.js
:: Starting Main Process...
:: 
:: > epi-info-maps@0.17.2 start-main-dev C:\_CODE\shed\epi-info-maps
:: > cross-env HOT=1 NODE_ENV=development electron -r @babel/register ./app/main.dev.js
:: 
:: ...and it stalls right here, try this
:: 
RD /S /Q C:\Users\ita3\AppData\Roaming\Electron
RD /S /Q C:\Users\ita3\AppData\Roaming\npm
:: 
:: ====================================================