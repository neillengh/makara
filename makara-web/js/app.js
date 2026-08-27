/* Makara Web — SPA shell, router, page loader, interactions */
(() => {
  'use strict';

  const STORE_AUTH = 'makara.auth';
  const STORE_THEME = 'makara.theme';
  const STORE_SERVER = 'makara.server';

  const $ = (sel, root = document) => root.querySelector(sel);
  const $$ = (sel, root = document) => Array.from(root.querySelectorAll(sel));

  const routes = {
    login: { view: 'login', navKey: null },
    dashboard: { view: 'dashboard', navKey: 'dashboard' },
    workflows: { view: 'workflows', navKey: 'workflows' },
    'workflow-canvas': { view: 'workflow-canvas', navKey: 'workflows' },
    'workflow-templates': { view: 'workflow-templates', navKey: 'workflows' },
    'data-sources': { view: 'data-sources', navKey: 'data-sources' },
    datasets: { view: 'datasets', navKey: 'datasets' },
    'field-mapping': { view: 'field-mapping', navKey: 'datasets' },
    runs: { view: 'runs', navKey: 'runs' },
    servers: { view: 'servers', navKey: 'servers' },
    settings: { view: 'settings', navKey: 'settings' },
  };

  /* ---------------- Theme ---------------- */
  function applyTheme(theme) {
    const html = document.documentElement;
    if (theme === 'light') { html.classList.add('light'); html.setAttribute('data-theme', 'light'); }
    else { html.classList.remove('light'); html.setAttribute('data-theme', 'dark'); }
    refreshThemeIcon();
  }
  function currentTheme() {
    return document.documentElement.classList.contains('light') ? 'light' : 'dark';
  }
  function refreshThemeIcon() {
    const btn = $('[aria-label="切换主题"]', $('#shell-header'));
    if (!btn) return;
    const i = $('i[data-lucide]', btn) || btn.querySelector('i');
    if (!i) return;
    const name = currentTheme() === 'light' ? 'moon' : 'sun';
    i.setAttribute('data-lucide', name);
    if (window.lucide) window.lucide.createIcons({ nameAttr: 'data-lucide', attrs: {} });
  }
  function toggleTheme() {
    const next = currentTheme() === 'light' ? 'dark' : 'light';
    localStorage.setItem(STORE_THEME, next);
    applyTheme(next);
    toast({ type: 'info', title: '主题已切换', desc: next === 'light' ? '当前：浅色' : '当前：深色' });
  }

  /* ---------------- Toast ---------------- */
  function toast({ type = 'info', title = '提示', desc = '', timeout = 2600 } = {}) {
    const wrap = $('#toast-wrap');
    const icons = { success: 'check-circle-2', error: 'x-circle', info: 'info', warning: 'alert-triangle' };
    const el = document.createElement('div');
    el.className = `mk-toast mk-toast--${type}`;
    el.innerHTML = `<i data-lucide="${icons[type] || 'info'}" class="mk-toast__icon"></i>
      <div><div class="mk-toast__title"></div>${desc ? '<div class="mk-toast__desc"></div>' : ''}</div>`;
    el.querySelector('.mk-toast__title').textContent = title;
    if (desc) el.querySelector('.mk-toast__desc').textContent = desc;
    wrap.appendChild(el);
    if (window.lucide) window.lucide.createIcons();
    requestAnimationFrame(() => el.setAttribute('data-show', 'true'));
    setTimeout(() => {
      el.removeAttribute('data-show');
      setTimeout(() => el.remove(), 300);
    }, timeout);
  }

  /* ---------------- Modal ---------------- */
  function openModal({ title = '', bodyHTML = '', footerHTML = '', size = 520, onMount } = {}) {
    const root = $('#modal-root');
    const back = document.createElement('div');
    back.className = 'mk-modal-backdrop';
    back.innerHTML = `
      <div class="mk-modal" style="max-width:${size}px">
        <div class="mk-modal__header">
          <div class="mk-modal__title"></div>
          <button class="mk-modal__close" aria-label="关闭"><i data-lucide="x"></i></button>
        </div>
        <div class="mk-modal__body"></div>
        <div class="mk-modal__footer"></div>
      </div>`;
    back.querySelector('.mk-modal__title').textContent = title;
    back.querySelector('.mk-modal__body').innerHTML = bodyHTML;
    back.querySelector('.mk-modal__footer').innerHTML = footerHTML;
    root.appendChild(back);
    if (window.lucide) window.lucide.createIcons();
    requestAnimationFrame(() => back.setAttribute('data-open', 'true'));

    const close = () => {
      back.removeAttribute('data-open');
      setTimeout(() => back.remove(), 160);
    };
    back.addEventListener('click', (e) => {
      if (e.target === back) close();
      if (e.target.closest('.mk-modal__close')) close();
    });
    document.addEventListener('keydown', function esc(e) {
      if (e.key === 'Escape') { close(); document.removeEventListener('keydown', esc); }
    });
    if (onMount) onMount(back, close);
    return { close, el: back };
  }

  function confirm({ title = '确认操作', message = '确定要执行此操作吗？', okText = '确认', cancelText = '取消', danger = false } = {}) {
    return new Promise((resolve) => {
      const m = openModal({
        title, size: 440,
        bodyHTML: `<p style="margin:0;color:var(--mk-text-secondary);font-size:14px;line-height:1.6;"></p>`,
        footerHTML: `<button class="mk-btn mk-btn--ghost" data-modal="cancel"></button>
                     <button class="mk-btn ${danger ? 'mk-btn--danger' : 'mk-btn--primary'}" data-modal="ok"></button>`,
        onMount: (el, close) => {
          el.querySelector('.mk-modal__body p').textContent = message;
          el.querySelector('[data-modal="cancel"]').textContent = cancelText;
          el.querySelector('[data-modal="ok"]').textContent = okText;
          el.querySelector('[data-modal="cancel"]').onclick = () => { close(); resolve(false); };
          el.querySelector('[data-modal="ok"]').onclick = () => { close(); resolve(true); };
        }
      });
    });
  }

  /* ---------------- Auth ---------------- */
  function isAuthed() { return localStorage.getItem(STORE_AUTH) === '1'; }
  function setAuthed(v) { v ? localStorage.setItem(STORE_AUTH, '1') : localStorage.removeItem(STORE_AUTH); }

  /* ---------------- Router ---------------- */
  function currentRoute() {
    const h = location.hash.replace(/^#\/?/, '').trim();
    return h || 'dashboard';
  }
  function navigate(route) {
    if (routes[route]) location.hash = '#/' + route;
    else location.hash = '#/dashboard';
  }

  async function render() {
    let route = currentRoute();
    if (route === 'login' && isAuthed()) { navigate('dashboard'); return; }
    if (!routes[route]) route = 'dashboard';
    if (route !== 'login' && !isAuthed()) { navigate('login'); return; }

    const showShell = route !== 'login';
    $('#app').hidden = !showShell;
    $('#login-root').hidden = showShell;

    // nav active
    const navKey = routes[route].navKey;
    $$('#shell-nav .mk-shell__nav-item').forEach(a => {
      a.setAttribute('data-active', String(a.getAttribute('data-nav-key') === navKey));
    });

    await loadView(routes[route].view);
  }

  /* ---------------- Page loader ---------------- */
  const SHARED_STYLE_IDS = new Set(['theme-vars', 'semantic-token-fallback']);

  async function fetchView(name) {
    const res = await fetch(`views/${name}.html`);
    if (!res.ok) throw new Error('view ' + name + ' not found');
    return await res.text();
  }

  async function loadView(name) {
    let html;
    try { html = await fetchView(name); }
    catch (e) { $('#view').innerHTML = '<p style="color:var(--mk-state-error)">视图加载失败：' + e.message + '</p>'; return; }

    const doc = new DOMParser().parseFromString(html, 'text/html');
    document.title = (doc.querySelector('title')?.textContent || 'Makara') + ' · Makara';

    // sandbox: styles + scripts
    const sandbox = $('#page-sandbox');
    sandbox.innerHTML = '';
    const styleHolder = document.createElement('div');
    styleHolder.style.display = 'none';
    sandbox.appendChild(styleHolder);

    const inlineScripts = [];
    $$('style', doc).forEach(st => {
      const id = st.getAttribute('id') || '';
      const type = st.getAttribute('type') || '';
      if (SHARED_STYLE_IDS.has(id)) return;
      if (type === 'text/tailwindcss') return;
      const clone = document.createElement('style');
      if (id) clone.id = id + '--view';
      clone.textContent = st.textContent;
      styleHolder.appendChild(clone);
    });
    $$('script', doc).forEach(s => { if (!s.hasAttribute('src')) inlineScripts.push(s.textContent); });

    if (name === 'login') {
      const card = doc.querySelector('.mk-login-card');
      $('#login-root').innerHTML = card ? card.outerHTML : '';
      // replace logo img with svg
      $$('img', $('#login-root')).forEach(img => {
        if (img.getAttribute('src')?.includes('logo')) {
          const span = document.createElement('span');
          span.style.cssText = 'display:inline-flex;align-items:center;';
          span.innerHTML = `<svg viewBox="0 0 40 40" fill="none" style="height:40px;width:40px;color:var(--mk-primary)"><rect width="40" height="40" rx="10" fill="currentColor" fill-opacity="0.12"/><path d="M8 29V13l12 8 12-8v16" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"/><circle cx="32" cy="29" r="2.4" fill="currentColor"/></svg>`;
          img.replaceWith(span);
        }
      });
      if (window.lucide) window.lucide.createIcons();
      inlineScripts.forEach(code => runScript(code));
      wireLogin();
      return;
    }

    // header + main
    const headerEl = doc.querySelector('.mk-shell__header');
    const mainEl = doc.querySelector('.mk-shell__main');
    $('#shell-header').innerHTML = headerEl ? headerEl.innerHTML : '';
    const view = $('#view');
    view.innerHTML = '';
    view.classList.remove('mk-view-enter');
    void view.offsetWidth;
    view.innerHTML = mainEl ? mainEl.innerHTML : '';
    view.classList.add('mk-view-enter');

    // replace logo img inside injected header (defensive)
    $$('img', $('#shell-header')).forEach(img => {
      if (img.getAttribute('src')?.includes('logo')) img.remove();
    });

    if (window.lucide) window.lucide.createIcons();
    wireHeader();
    wirePageInteractions(name, view);
    inlineScripts.forEach(code => runScript(code));
    if (window.lucide) window.lucide.createIcons();
    refreshThemeIcon();
  }

  function runScript(code) {
    try {
      const s = document.createElement('script');
      s.textContent = code;
      document.body.appendChild(s);
      s.remove();
    } catch (e) { console.warn('script run failed', e); }
  }

  /* ---------------- Header wiring ---------------- */
  function wireHeader() {
    const header = $('#shell-header');
    const themeBtn = $('[aria-label="切换主题"]', header);
    if (themeBtn) themeBtn.onclick = toggleTheme;
    const userBtn = $('[aria-label="用户菜单"]', header);
    if (userBtn) userBtn.onclick = () => toast({ type: 'info', title: '当前用户', desc: 'admin · 本地服务器' });
    const serverSel = header.querySelector('select');
    if (serverSel) {
      const saved = localStorage.getItem(STORE_SERVER);
      if (saved) serverSel.value = saved;
      serverSel.onchange = () => { localStorage.setItem(STORE_SERVER, serverSel.value); toast({ type: 'success', title: '服务端已切换', desc: serverSel.value }); };
    }
    const search = header.querySelector('input[type="search"]');
    if (search) search.addEventListener('input', () => filterTableByText(search.value));
  }

  /* ---------------- Login ---------------- */
  function wireLogin() {
    const root = $('#login-root');
    const form = root.querySelector('form');
    const btn = root.querySelector('[data-dom-id="btn-login"]');
    const serverInput = root.querySelector('[data-dom-id="input-server"]');
    const userInput = root.querySelector('[data-dom-id="input-username"]');
    const passInput = root.querySelector('[data-dom-id="input-password"]');
    const remember = root.querySelector('[data-dom-id="chk-remember"]');

    // restore remembered server
    const savedServer = localStorage.getItem('makara.serverurl');
    if (savedServer && serverInput) serverInput.value = savedServer;

    const submit = () => {
      if (!userInput?.value.trim()) { toast({ type: 'warning', title: '请输入用户名' }); userInput?.focus(); return; }
      if (!passInput?.value) { toast({ type: 'warning', title: '请输入密码' }); passInput?.focus(); return; }
      if (remember?.checked && serverInput) localStorage.setItem('makara.serverurl', serverInput.value);
      setAuthed(true);
      toast({ type: 'success', title: '登录成功', desc: '欢迎回来，admin' });
      setTimeout(() => navigate('dashboard'), 350);
    };
    if (form) form.onsubmit = (e) => { e.preventDefault(); submit(); };
    if (btn) btn.onclick = (e) => { e.preventDefault(); submit(); };
  }

  /* ---------------- Generic table filter ---------------- */
  function filterTableByText(q) {
    const view = $('#view');
    const ql = q.trim().toLowerCase();
    $$('table tbody tr', view).forEach(tr => {
      if (!ql) { tr.style.display = ''; return; }
      tr.style.display = tr.textContent.toLowerCase().includes(ql) ? '' : 'none';
    });
  }

  /* ---------------- Page interactions ---------------- */
  function wirePageInteractions(name, view) {
    // in-page search inputs
    $$('input[type="search"]', view).forEach(inp => {
      inp.addEventListener('input', () => filterTableByText(inp.value));
    });

    // generic tabs toggle
    $$('.mk-workflows__tab, .mk-tab, [role="tab"]', view).forEach(t => {
      t.addEventListener('click', () => {
        const group = t.parentElement;
        if (!group) return;
        $$(':scope > *', group).forEach(s => s.setAttribute('data-active', 'false'));
        t.setAttribute('data-active', 'true');
      });
    });

    if (name === 'workflow-canvas') initCanvas(view);
    if (name === 'settings') initSettings(view);
  }

  async function onPageClick(e) {
    const btn = e.target.closest('[data-dom-id]');
    if (!btn) return;
    const id = btn.getAttribute('data-dom-id') || '';

    // handled by dedicated handlers elsewhere
    if (id === 'btn-login' || id === 'btn-logout' || id.startsWith('nav-')) return;

    // delete (any id containing "delete") -> confirm + remove row
    if (id.includes('delete')) {
      e.preventDefault();
      const row = btn.closest('tr') || btn.closest('[data-row], .mk-card, .mk-server-card, li');
      const name = row?.querySelector('td, .mk-name, .title, [class*="name"]')?.textContent?.trim().split('\n')[0]
        || btn.getAttribute('title') || btn.getAttribute('aria-label') || '该项';
      const ok = await confirm({ title: '删除确认', message: `确定要删除「${name}」吗？此操作不可撤销。`, okText: '删除', danger: true });
      if (!ok) return;
      if (row) { row.setAttribute('data-removing', ''); setTimeout(() => { row.remove(); toast({ type: 'success', title: '已删除', desc: name }); }, 180); }
      else toast({ type: 'success', title: '已删除', desc: name });
      return;
    }

    // settings theme options
    if (id === 'theme-dark' || id === 'theme-light') {
      e.preventDefault();
      const group = btn.closest('.mk-option-group');
      if (group) $$(':scope > *', group).forEach(s => s.setAttribute('data-selected', 'false'));
      btn.setAttribute('data-selected', 'true');
      if (id === 'theme-dark') { localStorage.setItem(STORE_THEME, 'dark'); applyTheme('dark'); toast({ type: 'info', title: '主题已切换', desc: '当前：深色' }); }
      else { localStorage.setItem(STORE_THEME, 'light'); applyTheme('light'); toast({ type: 'info', title: '主题已切换', desc: '当前：浅色' }); }
      return;
    }
    // other option-group buttons (e.g. 界面缩放)
    if (btn.classList.contains('mk-option')) {
      e.preventDefault();
      const group = btn.closest('.mk-option-group');
      if (group) $$(':scope > *', group).forEach(s => s.setAttribute('data-selected', 'false'));
      btn.setAttribute('data-selected', 'true');
      toast({ type: 'info', title: '已选择', desc: btn.textContent.trim(), timeout: 1400 });
      return;
    }

    if (id.startsWith('workflow-edit-') || id === 'btn-edit') { e.preventDefault(); navigate('workflow-canvas'); return; }
    if (id.startsWith('workflow-run-') || id === 'btn-run') { e.preventDefault(); toast({ type: 'info', title: '工作流已开始运行', desc: '可在执行记录查看进度' }); return; }
    if (id === 'btn-new-workflow' || id === 'btn-new') { e.preventDefault(); openCreateWorkflowModal(); return; }
    if (id === 'btn-templates') { e.preventDefault(); navigate('workflow-templates'); return; }
    if (id === 'btn-back') { e.preventDefault(); history.back(); return; }
    if (id === 'btn-save') { e.preventDefault(); toast({ type: 'success', title: '已保存', desc: '工作流配置已更新' }); return; }
    if (id.startsWith('btn-test') || id === 'btn-test-connection') { e.preventDefault(); toast({ type: 'success', title: '连接测试成功', desc: '数据源可达，延迟 12ms' }); return; }
    if (id === 'btn-save-settings' || id.startsWith('btn-save')) { e.preventDefault(); toast({ type: 'success', title: '已保存', desc: btn.getAttribute('title') || '' }); return; }
    if (id.startsWith('btn-add') || id.startsWith('btn-new')) {
      e.preventDefault(); toast({ type: 'info', title: '新建', desc: '打开新建表单（演示）' }); return;
    }
    if (id.startsWith('btn-refresh')) { e.preventDefault(); toast({ type: 'info', title: '已刷新', desc: '数据已是最新', timeout: 1400 }); return; }
    if (id.startsWith('btn-export')) { e.preventDefault(); toast({ type: 'success', title: '导出已开始', desc: '文件将在后台生成' }); return; }
    if (id === 'btn-generate-dataset') { e.preventDefault(); toast({ type: 'info', title: '数据集生成中', desc: '可通过执行记录查看进度' }); return; }
    if (id.startsWith('btn-view')) { e.preventDefault(); toast({ type: 'info', title: '查看', desc: '打开详情预览（演示）' }); return; }
    if (id === 'btn-subscribe') { e.preventDefault(); toast({ type: 'success', title: 'SSE 已订阅', desc: '将接收实时进度推送' }); return; }
    if (id === 'btn-cancel-run') { e.preventDefault(); toast({ type: 'warning', title: '已请求取消', desc: '运行将在当前步骤后停止' }); return; }
    if (id === 'btn-reset-settings') { e.preventDefault(); toast({ type: 'info', title: '已恢复默认', desc: '点击保存后生效' }); return; }
    if (id === 'btn-check-update') { e.preventDefault(); toast({ type: 'info', title: '当前已是最新版本', desc: 'v0.1.0 MVP' }); return; }
    if (id === 'btn-preview' || id === 'btn-prev' || id === 'btn-next') {
      e.preventDefault(); toast({ type: 'info', title: btn.textContent.trim() || '操作', desc: '演示导航', timeout: 1200 }); return;
    }
    // generic fallback: any other btn-* gives feedback
    if (id.startsWith('btn-')) {
      e.preventDefault();
      toast({ type: 'info', title: btn.getAttribute('aria-label') || btn.getAttribute('title') || btn.textContent.trim() || '操作', desc: '该操作为演示', timeout: 1500 });
      return;
    }
    if (id.startsWith('kpi-') || id.startsWith('recent-run-') || id.startsWith('quick-')) {
      e.preventDefault();
      if (id.includes('workflow') || id.includes('run')) navigate('workflows');
      else if (id.includes('dataset')) navigate('datasets');
      else navigate('dashboard');
      return;
    }
  }

  function onPageInput(e) {
    const inp = e.target.closest('input[type="search"]');
    if (inp) filterTableByText(inp.value);
  }

  /* ---------------- Create workflow modal ---------------- */
  function openCreateWorkflowModal() {
    openModal({
      title: '新建工作流',
      bodyHTML: `
        <div class="mk-field">
          <label class="mk-field__label">工作流名称</label>
          <input class="mk-field__input" data-ml="name" placeholder="例如：每日销售报表生成" />
        </div>
        <div class="mk-field">
          <label class="mk-field__label">触发方式</label>
          <select class="mk-field__select" data-ml="trigger">
            <option value="manual">手动触发</option>
            <option value="schedule">定时触发</option>
            <option value="api">API 触发</option>
          </select>
        </div>
        <div class="mk-field">
          <label class="mk-field__label">描述（可选）</label>
          <textarea class="mk-field__textarea" data-ml="desc" placeholder="描述该工作流的用途..."></textarea>
        </div>`,
      footerHTML: `<button class="mk-btn mk-btn--ghost" data-ml="cancel">取消</button>
                   <button class="mk-btn mk-btn--primary" data-ml="create"><i data-lucide="plus" class="w-4 h-4"></i><span>创建并编排</span></button>`,
      onMount: (el, close) => {
        el.querySelector('[data-ml="cancel"]').onclick = close;
        el.querySelector('[data-ml="create"]').onclick = () => {
          const name = el.querySelector('[data-ml="name"]').value.trim() || '未命名工作流';
          close();
          toast({ type: 'success', title: '工作流已创建', desc: name });
          setTimeout(() => navigate('workflow-canvas'), 300);
        };
      }
    });
  }

  /* ---------------- Settings ---------------- */
  function initSettings(view) {
    // sync theme option selection to current theme
    const dark = currentTheme() === 'dark';
    $$('[data-dom-id="theme-dark"]', view).forEach(b => b.setAttribute('data-selected', String(dark)));
    $$('[data-dom-id="theme-light"]', view).forEach(b => b.setAttribute('data-selected', String(!dark)));
    // save buttons
    $$('button', view).forEach(b => {
      if (/save|保存/i.test(b.textContent)) b.addEventListener('click', () => toast({ type: 'success', title: '设置已保存' }));
    });
  }

  /* ---------------- Workflow canvas ---------------- */
  function initCanvas(view) {
    const center = $('.mk-workflow__center', view);
    const svg = $('.mk-canvas-connections', view);
    const rightTitle = $('.mk-workflow__right .mk-prop-group .text-sm', view);
    if (!center) return;

    const groupIcons = {
      Trigger: 'zap', DataSource: 'database', DataProcess: 'sliders-horizontal',
      Finetune: 'brain-circuit', Eval: 'check-circle', Deploy: 'rocket', Notify: 'bell'
    };

    // palette → drop
    $$('.mk-node-item', view).forEach(item => {
      item.addEventListener('dragstart', (e) => {
        const label = item.textContent.trim();
        const dot = item.querySelector('.mk-node-item__dot');
        const color = dot ? (dot.getAttribute('style').match(/background-color:\s*([^;]+)/)?.[1] || 'var(--mk-primary)') : 'var(--mk-primary)';
        const groupEl = item.closest('.mk-node-group');
        const group = groupEl ? groupEl.querySelector('.mk-node-group__label')?.textContent?.trim() : '';
        e.dataTransfer.setData('text/plain', JSON.stringify({ label, color, group }));
      });
    });
    center.addEventListener('dragover', (e) => { e.preventDefault(); });
    center.addEventListener('drop', (e) => {
      e.preventDefault();
      let data;
      try { data = JSON.parse(e.dataTransfer.getData('text/plain')); } catch (_) { return; }
      if (!data) return;
      const rect = center.getBoundingClientRect();
      const st = center.scrollTop;
      const x = e.clientX - rect.left + center.scrollLeft - 90; // half node width
      const y = e.clientY - rect.top + st - 24;
      const node = document.createElement('div');
      node.className = 'mk-canvas-node';
      node.style.cssText = `top:${Math.max(8, y)}px; left:${Math.max(8, x)}px; transform:none;`;
      node.setAttribute('data-node-id', 'node-' + Date.now());
      const icon = groupIcons[data.group] || 'circle';
      node.innerHTML = `
        <span class="mk-canvas-node__port mk-canvas-node__port--in"></span>
        <span class="mk-canvas-node__port mk-canvas-node__port--out"></span>
        <div class="mk-canvas-node__header">
          <div class="mk-canvas-node__icon" style="background-color:${data.color}"><i data-lucide="${icon}" class="w-4 h-4"></i></div>
          <div><div class="mk-canvas-node__title"></div><div class="mk-canvas-node__subtitle">${data.group || ''}</div></div>
        </div>`;
      node.querySelector('.mk-canvas-node__title').textContent = data.label;
      center.appendChild(node);
      if (window.lucide) window.lucide.createIcons();
      toast({ type: 'success', title: '节点已添加', desc: data.label, timeout: 1400 });
    });

    // move + select
    let drag = null;
    center.addEventListener('pointerdown', (e) => {
      const port = e.target.closest('.mk-canvas-node__port');
      if (port) { startConnect(port, e); return; }
      const node = e.target.closest('.mk-canvas-node');
      if (!node) return;
      selectNode(node);
      const rect = node.getBoundingClientRect();
      drag = { node, offX: e.clientX - rect.left, offY: e.clientY - rect.top };
      node.setPointerCapture(e.pointerId);
      e.stopPropagation();
    });
    center.addEventListener('pointermove', (e) => {
      if (!drag) return;
      const rect = center.getBoundingClientRect();
      const x = e.clientX - rect.left + center.scrollLeft - drag.offX;
      const y = e.clientY - rect.top + center.scrollTop - drag.offY;
      drag.node.style.left = Math.max(4, x) + 'px';
      drag.node.style.top = Math.max(4, y) + 'px';
      drag.node.style.transform = 'none';
      redrawConnections();
    });
    center.addEventListener('pointerup', (e) => {
      if (drag) { try { drag.node.releasePointerCapture(e.pointerId); } catch (_) {} drag = null; }
      finishConnect(e);
    });

    function selectNode(node) {
      $$('.mk-canvas-node', center).forEach(n => n.setAttribute('data-selected', 'false'));
      node.setAttribute('data-selected', 'true');
      if (rightTitle) {
        const t = node.querySelector('.mk-canvas-node__title')?.textContent || '';
        rightTitle.textContent = t;
      }
    }

    // click-to-connect via ports
    let pending = null;
    function startConnect(port, e) {
      e.stopPropagation();
      const node = port.closest('.mk-canvas-node');
      if (port.classList.contains('mk-canvas-node__port--out')) {
        pending = { fromNode: node, fromPort: port };
        toast({ type: 'info', title: '请点击目标节点的输入端口', timeout: 2000 });
      } else if (pending && port.classList.contains('mk-canvas-node__port--in')) {
        addConnection(pending.fromNode, node);
        pending = null;
      }
    }
    function finishConnect(e) {
      // handled in startConnect for ports
    }

    const connections = [];
    function portCenter(node, which) {
      const port = node.querySelector(which === 'out' ? '.mk-canvas-node__port--out' : '.mk-canvas-node__port--in');
      if (!port) return null;
      const pr = port.getBoundingClientRect();
      const cr = center.getBoundingClientRect();
      return { x: pr.left + pr.width / 2 - cr.left + center.scrollLeft, y: pr.top + pr.height / 2 - cr.top + center.scrollTop };
    }
    function addConnection(from, to) {
      if (from === to) return;
      connections.push({ from, to });
      redrawConnections();
      toast({ type: 'success', title: '已连接节点', desc: `${from.querySelector('.mk-canvas-node__title')?.textContent} → ${to.querySelector('.mk-canvas-node__title')?.textContent}`, timeout: 1400 });
    }
    function redrawConnections() {
      if (!svg) return;
      // keep original decorative paths, remove dynamic
      $$('path[data-dyn="1"]', svg).forEach(p => p.remove());
      const W = center.scrollWidth || 1000, H = center.scrollHeight || 700;
      const vb = (svg.getAttribute('viewBox') || '0 0 1000 700').split(' ').map(Number);
      const sx = vb[2] / W, sy = vb[3] / H;
      connections.forEach(c => {
        const a = portCenter(c.from, 'out'), b = portCenter(c.to, 'in');
        if (!a || !b) return;
        const d = `M${a.x * sx} ${a.y * sy} C${a.x * sx} ${(a.y + 30) * sy} ${b.x * sx} ${(b.y - 30) * sy} ${b.x * sx} ${b.y * sy}`;
        const path = document.createElementNS('http://www.w3.org/2000/svg', 'path');
        path.setAttribute('d', d);
        path.setAttribute('data-dyn', '1');
        svg.appendChild(path);
      });
    }
  }

  /* ---------------- Global events ---------------- */
  // single delegated click handler for all [data-dom-id] buttons across shell + view
  document.addEventListener('click', onPageClick);

  $('#shell-nav').addEventListener('click', (e) => {
    const a = e.target.closest('[data-nav-key]');
    if (!a) return;
    e.preventDefault();
    navigate(a.getAttribute('data-nav-key'));
  });
  $('#btn-logout').addEventListener('click', async () => {
    const ok = await confirm({ title: '退出登录', message: '确定要退出当前会话吗？', okText: '退出' });
    if (!ok) return;
    setAuthed(false);
    navigate('login');
  });

  window.addEventListener('hashchange', render);

  /* ---------------- Boot ---------------- */
  applyTheme(localStorage.getItem(STORE_THEME) || 'dark');
  if (!isAuthed()) { navigate('login'); }
  else if (!location.hash) { navigate('dashboard'); }
  else { render(); }

  // expose minimal API for inline page scripts that may need helpers
  window.Makara = { toast, openModal, confirm, navigate, toggleTheme };
})();
