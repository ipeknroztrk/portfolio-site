import { GameComponent } from './game.component';
import { LanguageService } from '../../../core/services/language.service';

const SESSION_URL = 'https://api.ipekozturk.com/api/leaderboard/session';
const SCORE_URL = 'https://api.ipekozturk.com/api/leaderboard';

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } });
}

describe('GameComponent score submission', () => {
  let component: GameComponent;
  let fetchSpy: jasmine.Spy;

  // Oyunu gerçekten oynatmadan: oturumu başlat, skoru ayarla, gönder
  async function finishGameWith(points: number): Promise<void> {
    const anyComponent = component as any;
    anyComponent.gameSession = anyComponent.startGameSession();
    component.playerName = 'ipek';
    component.score = points;
    await component.sendScore();
  }

  function scorePosts(): jasmine.CallInfo<typeof fetch>[] {
    return fetchSpy.calls.all().filter(c => c.args[0] === SCORE_URL && c.args[1]?.method === 'POST');
  }

  beforeEach(() => {
    component = new GameComponent({ current: () => 'tr' } as unknown as LanguageService);
    fetchSpy = spyOn(window, 'fetch');
  });

  it('sends the server-issued session id with the score', async () => {
    fetchSpy.and.callFake(async (url: RequestInfo | URL) =>
      url === SESSION_URL ? jsonResponse({ sessionId: 'abc123' }) : jsonResponse({ success: true }));

    await finishGameWith(500);

    const posts = scorePosts();
    expect(posts.length).toBe(1);
    expect(JSON.parse(posts[0].args[1]!.body as string)).toEqual({ sessionId: 'abc123', playerName: 'ipek', points: 500 });
  });

  it('does not submit the score when session creation returns an error status', async () => {
    fetchSpy.and.callFake(async (url: RequestInfo | URL) =>
      url === SESSION_URL ? jsonResponse({ error: 'nope' }, 404) : jsonResponse({ success: true }));

    await finishGameWith(500);

    expect(scorePosts().length).toBe(0);
  });

  it('does not submit the score when session creation throws (network error)', async () => {
    fetchSpy.and.callFake(async (url: RequestInfo | URL) => {
      if (url === SESSION_URL) throw new TypeError('Failed to fetch');
      return jsonResponse({ success: true });
    });

    await finishGameWith(500);

    expect(scorePosts().length).toBe(0);
  });

  it('does not submit the score when the session response has no id', async () => {
    fetchSpy.and.callFake(async (url: RequestInfo | URL) =>
      url === SESSION_URL ? jsonResponse({}) : jsonResponse({ success: true }));

    await finishGameWith(500);

    expect(scorePosts().length).toBe(0);
  });

  it('never sends a score request without a sessionId field', async () => {
    fetchSpy.and.callFake(async (url: RequestInfo | URL) =>
      url === SESSION_URL ? jsonResponse({ sessionId: 's1' }) : jsonResponse({ success: true }));

    await finishGameWith(120);
    await finishGameWith(0);   // sıfır skor hiç gönderilmez

    for (const call of scorePosts()) {
      expect(JSON.parse(call.args[1]!.body as string).sessionId).toBeTruthy();
    }
    expect(scorePosts().length).toBe(1);
  });
});
