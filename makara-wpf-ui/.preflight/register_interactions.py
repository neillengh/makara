import json, os

base = r'e:\WorkSpace\Trae WorkSpace\openname\makara-wpf-ui'
design_path = os.path.join(base, 'makara-wpf-ui.design')
summary_path = os.path.join(base, 'runtime-orchestration-summary.json')

nav_items = [
    ('nav-dashboard', 'page-dashboard'),
    ('nav-workflows', 'page-workflows'),
    ('nav-data-sources', 'page-data-sources'),
    ('nav-datasets', 'page-datasets'),
    ('nav-runs', 'page-runs'),
    ('nav-servers', 'page-servers'),
    ('nav-settings', 'page-settings'),
]

def nav_interactions():
    return [{'domId': d, 'targetPageId': t, 'hideEdge': True, 'transitionLabel': '侧边栏导航'} for d, t in nav_items]

interactions_by_page = {
    'page-login': [
        {'domId': 'btn-login', 'targetPageId': 'page-dashboard', 'transitionLabel': '登录成功'}
    ],
    'page-dashboard': nav_interactions() + [
        {'domId': 'btn-new-workflow', 'targetPageId': 'page-workflows', 'transitionLabel': '新建工作流'}
    ],
    'page-workflows': nav_interactions() + [
        {'domId': 'btn-new-workflow', 'targetPageId': 'page-workflow-canvas', 'transitionLabel': '进入画布'},
        {'domId': 'btn-templates', 'targetPageId': 'page-workflow-templates', 'hideEdge': True, 'transitionLabel': '模板库'}
    ],
    'page-workflow-canvas': nav_interactions() + [
        {'domId': 'btn-back', 'targetPageId': 'page-workflows', 'hideEdge': True, 'transitionLabel': '返回列表'},
        {'domId': 'btn-run', 'targetPageId': 'page-runs', 'hideEdge': True, 'transitionLabel': '查看运行'}
    ],
    'page-workflow-templates': nav_interactions() + [
        {'domId': 'use-template-1', 'targetPageId': 'page-workflow-canvas', 'hideEdge': True, 'transitionLabel': '套用模板'}
    ],
    'page-data-sources': nav_interactions(),
    'page-field-mapping': nav_interactions(),
    'page-datasets': nav_interactions(),
    'page-runs': nav_interactions(),
    'page-servers': nav_interactions(),
    'page-settings': nav_interactions(),
}

with open(design_path, 'r', encoding='utf-8') as f:
    design = json.load(f)

page_ids = {node['id'] for node in design['data'] if node.get('type') == 'page'}
expected_dom_ids = []
missing = []

for node in design['data']:
    if node.get('type') != 'page':
        continue
    page_id = node['id']
    interactions = interactions_by_page.get(page_id, [])
    # ensure targetPageId exists
    for inter in interactions:
        if inter['targetPageId'] not in page_ids:
            missing.append(f"{page_id} -> {inter['targetPageId']}")
        expected_dom_ids.append(f"{inter['domId']}:{page_id}")
    node.setdefault('devMetadata', {})['interactions'] = interactions

with open(design_path, 'w', encoding='utf-8') as f:
    json.dump(design, f, ensure_ascii=False, indent=2)

# update summary
with open(summary_path, 'r', encoding='utf-8') as f:
    summary = json.load(f)

summary.setdefault('project', {})['wiringRegistrationEvidence'] = {
    'expectedDomIds': expected_dom_ids,
    'allExpectedDomIdsRegistered': len(missing) == 0,
    'missingDomIds': missing
}

with open(summary_path, 'w', encoding='utf-8') as f:
    json.dump(summary, f, ensure_ascii=False, indent=2)

print('interactions registered', len(expected_dom_ids), 'missing', missing)