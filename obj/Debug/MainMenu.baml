   M S B A M L   `   `   `  ����C  ?CityNest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:CityNestCityNest  5   	       CityNest.MainMenu   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   (localclr-namespace:CityNest     5   	     � Title$  Меню Риэлтора��5   	   $	��760��6   $
��1200��6,    � WindowStartupLocation$ CenterScreen=�5	   	    � WindowStyle$
 None=�5
   	   
������  � AllowsTransparency . .��5      �� ���#ccccff �� CornerRadius$ 20��6"   ._�5   
   �  � ColumnDefinitions 5      �� 5      $	��200 �6#   �� 5      $��* �6#   .+�5      �� $��0��6   ���#7070ff$	��200��6?   .+�5      ��  �� Orientation$ 
Horizontal=�6   $��60��66   ���#7070ff$��0 0 0 30q�6W   .+�5      �� $��CityNest��6    
�������$��22(�6C   $
��Bold"�6Q   $��Center=�6c   $��10 0 0 0q�6~   5      �� 5      -   5        $��Квартиры��5         
MenuButton#��[  6,   
 � Tag$ 
Apartments��6P   ���#7070ff6z   �� 5      -   5        $��Покупатели��5      #��[  6.   $ Buyers��6R   ���#7070ff6x   �� 5      -   5        $��Сделки��5      #��[  6*   $ Transactions��6N   ���#7070ff6z   �� 5      -   5        $��Отчеты��5      #��[  6*   $ Reports��6N   ���#7070ff6u   �� 5"      ���#3E3B4F$��10 20q�61   �� 5%      -   5        $)��#Настройки 
Профиля��5%      #��[  6?   $ Settings��6c   $��68��6�   ���#7070ff6�   �� 5'      ���#3E3B4F$	��107��61   $	��179��6>   �� 5(      -   5        $��
Выйти��5(      #��[  6)   $ Logout��6M   ���#7070ff6s   5)      � 5,      $��1��6   $��20 20 20 20q�6#   .+�5-      r� -   5         r�Name$ BackgroundVideo��5-      	 r� Source$	 Folder/video.mp4?�5.      
 r� Stretch$
 UniformToFill=�5/       r� LoadedBehavior$ Manual=�50       r� UnloadedBehavior$
 Stop=�51       r� IsMuted . $y�Visible=�54       J� Opacity$	 0.3Q�55       � 56      -   5          �Name$ ContentFrame��56        � NavigationUIVisibility$ Hidden=�6.   
������ 6N   57      5:      5;      5<      