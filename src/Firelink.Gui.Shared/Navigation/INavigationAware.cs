namespace Firelink.Gui.Shared.Navigation;

/// <summary>
/// VM, которым нужно вернуться на Home.
///
/// MainWindowVM при навигации проверяет: если VM реализует INavigationAware,
/// передаёт ей callback для возврата на главный экран.
///
/// Реализуется плейсхолдерами (3.2–3.3) и настоящими экранами
/// (InstallVM в 3.4.2, PackVM в 3.5, VerifyVM в 3.6).
///
/// Идемпотентность: SetNavigateHome можно вызывать многократно.
/// Повторный вызов перезаписывает callback.
/// </summary>
public interface INavigationAware
{
    void SetNavigateHome(Action navigateHome);
}
