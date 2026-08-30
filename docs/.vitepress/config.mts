import { defineConfig } from 'vitepress'

// GitHub Pages serves the site under /<repo>/, override with DOCS_BASE when needed.
const base = process.env.DOCS_BASE ?? '/Material3.Avalonia/'

export default defineConfig({
  title: 'Material3.Avalonia',
  description: 'Material Design 3 theme for Avalonia — standalone, dynamic color, 80+ controls',
  base,
  lang: 'en-US',
  lastUpdated: true,

  themeConfig: {
    nav: [
      { text: 'Guide', link: '/guide/getting-started' },
      { text: 'Controls', link: '/controls/overview' },
      { text: 'Live Demo (WASM)', link: base + 'demo/', target: '_blank' },
    ],

    sidebar: {
      '/guide/': [
        {
          text: 'Guide',
          items: [
            { text: 'Getting Started', link: '/guide/getting-started' },
            { text: 'Theme Configuration', link: '/guide/theme-configuration' },
            { text: 'Color System', link: '/guide/color-system' },
            { text: 'Typography & Tokens', link: '/guide/tokens' },
            { text: 'Material Symbols', link: '/guide/icons' },
            { text: 'Platforms (Desktop / Mobile / WASM)', link: '/guide/platforms' },
          ],
        },
      ],
      '/controls/': [
        {
          text: 'Controls',
          items: [
             { text: 'Overview & Style Classes', link: '/controls/overview' },
            { text: 'Chips', link: '/controls/chips' },
            { text: 'Buttons, FABs & Groups', link: '/controls/buttons-fabs' },
            { text: 'Navigation', link: '/controls/navigation' },
            { text: 'App Bars & Toolbar', link: '/controls/app-bars' },
            { text: 'Cards, Avatars & Badges', link: '/controls/cards-badges' },
            { text: 'Sheets, Scrim & Dialogs', link: '/controls/sheets-dialogs' },
            { text: 'Snackbar, Tooltip & Search', link: '/controls/feedback-search' },
            { text: 'Settings Items', link: '/controls/settings' },
             { text: 'Sliders & RangeSlider', link: '/controls/sliders' },
            { text: 'Progress Indicators', link: '/controls/progress' },
            { text: 'Time Picker (Dial & Pane)', link: '/controls/time-picker' },
            { text: 'Ripple & Entrance Animations', link: '/controls/primitives' },
          ],
        },
      ],
    },

    socialLinks: [
      { icon: 'github', link: 'https://github.com/greepar/Material3.Avalonia' },
    ],

    outline: { level: [2, 3] },
    search: { provider: 'local' },
  },
})
