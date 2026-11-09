(() => {
  if (window !== window.top || window.__podcastDesktop) return;

  const click = (selector) => {
    const el = document.querySelector(selector);
    if (!el) return false;
    el.click();
    return true;
  };

  const audio = () => document.querySelector('audio');

  const postState = () => {
    const a = audio();
    const meta = navigator.mediaSession && navigator.mediaSession.metadata;
    window.chrome.webview.postMessage({
      type: 'state',
      playing: !!a && !a.paused,
      title: meta ? meta.title : '',
      artist: meta ? meta.artist : ''
    });
  };

  ['play', 'pause', 'ended', 'emptied'].forEach((type) =>
    document.addEventListener(type, (e) => {
      if (e.target instanceof HTMLMediaElement) postState();
    }, true));

  const navLabels = { home: '主页', latest: '最新单集', shows: '节目' };

  window.__podcastDesktop = {
    toggle() {
      const a = audio();
      return a && !a.paused
        ? click('button.playback-play__pause')
        : click('button.playback-play__play');
    },
    back: () => click('[data-testid="playback-controls-skip-back"]'),
    forward: () => click('[data-testid="playback-controls-skip-forward"]'),
    navigate(page) {
      const label = navLabels[page];
      const btn = [...document.querySelectorAll('nav button, [role="navigation"] button')]
        .find((b) => b.textContent.trim() === label);
      if (!btn) return false;
      btn.click();
      return true;
    }
  };
})();
